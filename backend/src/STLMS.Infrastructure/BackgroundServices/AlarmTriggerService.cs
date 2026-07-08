using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.BackgroundServices;

/// <summary>Polls every 20s for alarms whose Hour:Minute (in the owning user's own TimezoneId)
/// matches right now, and dispatches a notification through INotificationDispatcher. This is the
/// server-side record of truth (and foundation for real push delivery in the Smart Notifications
/// milestone) - actually *ringing* a sound is a client-side concern (see the frontend's alarm
/// ringing overlay), since a server has no way to make a browser tab make noise on its own.</summary>
public class AlarmTriggerService(IServiceScopeFactory scopeFactory, ILogger<AlarmTriggerService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DedupeWindow = TimeSpan.FromSeconds(90);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAlarmsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AlarmTriggerService check failed");
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

    private async Task CheckAlarmsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var enabledAlarms = await uow.Repository<Alarm>().FindAsync(a => a.IsEnabled, ct);
        if (enabledAlarms.Count == 0) return;

        var userIds = enabledAlarms.Select(a => a.UserId).ToHashSet();
        var users = (await uow.Repository<User>().FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        foreach (var alarm in enabledAlarms)
        {
            if (!users.TryGetValue(alarm.UserId, out var user)) continue;

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

            if (nowInUserZone.Hour != alarm.Hour || nowInUserZone.Minute != alarm.Minute) continue;

            var todayMask = AlarmDayMask.ForDayOfWeek(nowInUserZone.DayOfWeek);
            var isScheduledToday = alarm.RepeatDaysMask == 0 || (alarm.RepeatDaysMask & todayMask) != 0;
            if (!isScheduledToday) continue;

            // Computed as a plain variable rather than inline (`DateTime.UtcNow - DedupeWindow`)
            // inside the LINQ predicate - EF Core's query translator can't turn that arithmetic
            // into SQL and throws InvalidOperationException at query time. A precomputed constant
            // just gets parameterized, which every provider supports.
            var cutoff = DateTime.UtcNow - DedupeWindow;
            var recentlyFired = await uow.Repository<AlarmHistory>().FindAsync(
                h => h.AlarmId == alarm.Id && h.Action == AlarmHistoryAction.Triggered && h.OccurredAt > cutoff, ct);
            if (recentlyFired.Count > 0) continue;

            await uow.Repository<AlarmHistory>().AddAsync(
                new AlarmHistory { AlarmId = alarm.Id, UserId = alarm.UserId, Action = AlarmHistoryAction.Triggered }, ct);
            await uow.SaveChangesAsync(ct);

            await dispatcher.DispatchAsync(alarm.UserId, "Alarm", alarm.Label, ct);
        }
    }
}
