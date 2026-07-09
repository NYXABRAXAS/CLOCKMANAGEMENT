using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.BackgroundServices;

/// <summary>Same shape as AlarmTriggerService, one poll level up: checks every active Medicine's
/// MedicineTime slots against each owner's local time, and dispatches a reminder the first time a
/// slot matches "now" on a scheduled day. MedicineReminderLog (rather than MedicineLog, which
/// records the user's own taken/skipped action) is the dedupe record, playing the same role
/// AlarmHistory plays for alarms. Polls every 60s rather than 20s - medicine reminders tolerate a
/// slightly wider trigger window than alarms without being noticeably late.</summary>
public class MedicineReminderService(IServiceScopeFactory scopeFactory, ILogger<MedicineReminderService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckMedicinesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MedicineReminderService check failed");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected on shutdown.
            }
        }
    }

    private async Task CheckMedicinesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var activeMedicines = await uow.Repository<Medicine>().FindAsync(m => m.IsActive, ct);
        if (activeMedicines.Count == 0) return;

        var medicineIds = activeMedicines.Select(m => m.Id).ToHashSet();
        var times = (await uow.Repository<MedicineTime>().FindAsync(t => medicineIds.Contains(t.MedicineId), ct))
            .GroupBy(t => t.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

        var userIds = activeMedicines.Select(m => m.UserId).ToHashSet();
        var users = (await uow.Repository<User>().FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        foreach (var medicine in activeMedicines)
        {
            if (!users.TryGetValue(medicine.UserId, out var user)) continue;
            if (!times.TryGetValue(medicine.Id, out var medicineTimes)) continue;

            DateTime nowInUserZone;
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(user.TimezoneId);
                nowInUserZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                nowInUserZone = DateTime.UtcNow;
            }

            var today = DateOnly.FromDateTime(nowInUserZone);
            if (today < medicine.StartDate || (medicine.EndDate is { } end && today > end)) continue;

            var todayMask = AlarmDayMask.ForDayOfWeek(nowInUserZone.DayOfWeek);
            var isScheduledToday = medicine.RepeatDaysMask == 0 || (medicine.RepeatDaysMask & todayMask) != 0;
            if (!isScheduledToday) continue;

            foreach (var time in medicineTimes)
            {
                if (nowInUserZone.Hour != time.Hour || nowInUserZone.Minute != time.Minute) continue;

                var alreadySent = await uow.Repository<MedicineReminderLog>().FindAsync(
                    l => l.MedicineId == medicine.Id && l.Date == today && l.Hour == time.Hour && l.Minute == time.Minute, ct);
                if (alreadySent.Count > 0) continue;

                await uow.Repository<MedicineReminderLog>().AddAsync(
                    new MedicineReminderLog { MedicineId = medicine.Id, UserId = medicine.UserId, Date = today, Hour = time.Hour, Minute = time.Minute },
                    ct);
                await uow.SaveChangesAsync(ct);

                var dosageSuffix = string.IsNullOrWhiteSpace(medicine.Dosage) ? "" : $" ({medicine.Dosage})";
                await dispatcher.DispatchAsync(medicine.UserId, "Medicine reminder", $"Time to take {medicine.Name}{dosageSuffix}", ct);
            }
        }
    }
}
