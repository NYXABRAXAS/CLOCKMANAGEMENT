using STLMS.Application.Common.Mediator;
using STLMS.Application.Productivity.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Productivity.Queries;

public record GetProductivitySummaryQuery(Guid UserId, DateOnly From, DateOnly To) : IRequest<ProductivitySummaryDto>;

public class GetProductivitySummaryQueryHandler(IUnitOfWork uow) : IRequestHandler<GetProductivitySummaryQuery, ProductivitySummaryDto>
{
    private const double ProductiveDayThreshold = 60;

    public async Task<ProductivitySummaryDto> HandleAsync(GetProductivitySummaryQuery request, CancellationToken ct)
    {
        var userId = request.UserId;
        var from = request.From;
        var to = request.To;

        var user = await uow.Repository<User>().GetByIdAsync(userId, ct);
        var religion = user?.ReligionId is { } religionId ? await uow.Repository<Religion>().GetByIdAsync(religionId, ct) : null;
        var prayersApplicable = religion?.Code == ReligionCodes.Islam;

        var allHabits = await uow.Repository<Habit>().FindAsync(h => h.UserId == userId && h.IsActive, ct);
        var habitLogs = await uow.Repository<HabitLog>().FindAsync(l => l.UserId == userId && l.Date >= from && l.Date <= to, ct);
        var completedHabitDates = habitLogs.Where(l => l.Completed).Select(l => (l.HabitId, l.Date)).ToHashSet();

        var allMedicines = await uow.Repository<Medicine>().FindAsync(m => m.UserId == userId && m.IsActive, ct);
        var medicineIds = allMedicines.Select(m => m.Id).ToHashSet();
        var medicineTimes = (await uow.Repository<MedicineTime>().FindAsync(t => medicineIds.Contains(t.MedicineId), ct))
            .GroupBy(t => t.MedicineId).ToDictionary(g => g.Key, g => g.Count());
        var medicineLogs = await uow.Repository<MedicineLog>().FindAsync(
            l => l.UserId == userId && l.ScheduledDate >= from && l.ScheduledDate <= to && l.Status == Domain.Enums.MedicineLogStatus.Taken, ct);
        var takenDoseCount = medicineLogs.GroupBy(l => l.ScheduledDate).ToDictionary(g => g.Key, g => g.Count());

        var sleepLogs = (await uow.Repository<SleepLog>().FindAsync(s => s.UserId == userId && s.Date >= from && s.Date <= to, ct))
            .ToDictionary(s => s.Date);
        var allSleepLogs = await uow.Repository<SleepLog>().FindAsync(s => s.UserId == userId, ct);
        DateOnly? sleepAdoptedDate = allSleepLogs.Count > 0 ? allSleepLogs.Min(s => s.Date) : null;

        var pomodoroSessions = await uow.Repository<PomodoroSession>().FindAsync(s => s.UserId == userId, ct);
        var sessionIds = pomodoroSessions.Select(s => s.Id).ToHashSet();
        var focusMinutesByDate = (await uow.Repository<PomodoroLog>().FindAsync(
                l => sessionIds.Contains(l.PomodoroSessionId) && l.Phase == Domain.Enums.PomodoroPhase.Work && l.CompletedFully, ct))
            .Where(l => DateOnly.FromDateTime(l.StartedAt) >= from && DateOnly.FromDateTime(l.StartedAt) <= to)
            .GroupBy(l => DateOnly.FromDateTime(l.StartedAt))
            .ToDictionary(g => g.Key, g => (int)g.Sum(l => (l.EndedAt - l.StartedAt).TotalMinutes));
        DateOnly? pomodoroAdoptedDate = pomodoroSessions.Count > 0 ? pomodoroSessions.Min(s => DateOnly.FromDateTime(s.StartedAt)) : null;

        var allPrayerLogs = prayersApplicable ? await uow.Repository<UserPrayerLog>().FindAsync(p => p.UserId == userId, ct) : [];
        DateOnly? prayersAdoptedDate = allPrayerLogs.Count > 0 ? allPrayerLogs.Min(p => p.Date) : null;
        var prayerLogs = allPrayerLogs.Where(p => p.Completed && p.Date >= from && p.Date <= to).ToList();
        var prayerCountByDate = prayerLogs.GroupBy(p => p.Date).ToDictionary(g => g.Key, g => g.Select(p => p.PrayerName).Distinct().Count());

        var days = new List<ProductivityDayDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var scheduledHabits = allHabits.Where(h =>
                DateOnly.FromDateTime(h.CreatedAt) <= date && ProductivityScoreCalculator.IsScheduled(h.RepeatDaysMask, date.DayOfWeek)).ToList();
            double? habitsPercent = scheduledHabits.Count > 0
                ? scheduledHabits.Count(h => completedHabitDates.Contains((h.Id, date))) / (double)scheduledHabits.Count * 100
                : null;

            var scheduledMedicines = allMedicines.Where(m =>
                m.StartDate <= date && (m.EndDate == null || m.EndDate >= date) &&
                ProductivityScoreCalculator.IsScheduled(m.RepeatDaysMask, date.DayOfWeek)).ToList();
            var scheduledDoseCount = scheduledMedicines.Sum(m => medicineTimes.GetValueOrDefault(m.Id, 0));
            double? medicinesPercent = scheduledDoseCount > 0
                ? Math.Min(100, takenDoseCount.GetValueOrDefault(date, 0) / (double)scheduledDoseCount * 100)
                : null;

            var sleepDayApplicable = sleepAdoptedDate is { } sd && date >= sd;
            var pomodoroDayApplicable = pomodoroAdoptedDate is { } pd && date >= pd;
            var prayersDayApplicable = prayersApplicable && prayersAdoptedDate is { } prd && date >= prd;
            double? prayersPercent = prayersDayApplicable ? Math.Min(100, prayerCountByDate.GetValueOrDefault(date, 0) / 5.0 * 100) : null;

            days.Add(ProductivityScoreCalculator.ScoreDay(
                date,
                habitsPercent,
                medicinesPercent,
                sleepLogs.GetValueOrDefault(date),
                focusMinutesByDate.GetValueOrDefault(date, 0),
                sleepDayApplicable,
                pomodoroDayApplicable,
                prayersDayApplicable,
                prayersPercent));
        }

        var scoredDays = days.Where(d => d.Score is not null).ToList();
        var averageScore = scoredDays.Count > 0 ? scoredDays.Average(d => d.Score!.Value) : (double?)null;

        var currentStreak = 0;
        for (var i = days.Count - 1; i >= 0; i--)
        {
            if (days[i].Score is { } sc && sc >= ProductiveDayThreshold) currentStreak++;
            else break;
        }

        var bestStreak = 0;
        var running = 0;
        foreach (var day in days)
        {
            if (day.Score is { } sc && sc >= ProductiveDayThreshold)
            {
                running++;
                bestStreak = Math.Max(bestStreak, running);
            }
            else
            {
                running = 0;
            }
        }

        return new ProductivitySummaryDto(
            days,
            averageScore,
            currentStreak,
            bestStreak,
            days.Sum(d => d.Components.FocusMinutes),
            habitLogs.Count(l => l.Completed),
            prayerLogs.Count);
    }
}
