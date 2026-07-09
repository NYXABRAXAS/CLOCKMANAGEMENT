using STLMS.Application.Productivity.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.Productivity;

/// <summary>Turns a day's raw activity into a 0-100 score. A component only counts toward the
/// average if it's "applicable" - habits/medicines are applicable on a given day only if
/// something was actually scheduled that day (a rest day shouldn't be scored as 0% habits done),
/// while sleep/pomodoro/prayers are applicable for every day once the user has adopted that
/// module at all (no activity that day is a real, honest 0 - unlike habits there's no schedule to
/// be "off" from). A day with zero applicable components has a null score rather than a
/// misleading 0 or 100.</summary>
public static class ProductivityScoreCalculator
{
    private const double FocusMinutesForFullScore = 90;
    private const double SleepTargetMinutes = 480; // 8 hours
    private const double SleepScorePerMinuteOff = 100.0 / 180.0; // score hits 0 at 3h away from target

    public static bool IsScheduled(int repeatDaysMask, DayOfWeek day) =>
        repeatDaysMask == 0 || (repeatDaysMask & AlarmDayMask.ForDayOfWeek(day)) != 0;

    public static ProductivityDayDto ScoreDay(
        DateOnly date,
        double? habitsPercent,
        double? medicinesPercent,
        SleepLog? sleepLog,
        int focusMinutes,
        bool sleepModuleUsed,
        bool pomodoroModuleUsed,
        bool prayersApplicable,
        double? prayersPercent)
    {
        double? sleepScore = sleepModuleUsed ? ScoreSleep(sleepLog) : null;
        double? pomodoroScore = pomodoroModuleUsed ? Math.Min(100, focusMinutes / FocusMinutesForFullScore * 100) : null;

        var components = new List<double>();
        if (habitsPercent is { } h) components.Add(h);
        if (medicinesPercent is { } m) components.Add(m);
        if (sleepScore is { } s) components.Add(s);
        if (pomodoroScore is { } p) components.Add(p);
        if (prayersApplicable && prayersPercent is { } pr) components.Add(pr);

        var score = components.Count > 0 ? components.Average() : (double?)null;

        return new ProductivityDayDto(
            date,
            score,
            new ProductivityComponentsDto(habitsPercent, medicinesPercent, sleepScore, focusMinutes, prayersApplicable ? prayersPercent : null));
    }

    private static double ScoreSleep(SleepLog? log)
    {
        if (log is null) return 0;
        var minutesOff = Math.Abs(log.DurationMinutes - SleepTargetMinutes);
        var durationScore = Math.Max(0, 100 - minutesOff * SleepScorePerMinuteOff);
        if (log.Quality is not { } quality) return durationScore;
        var qualityScore = (int)quality / 3.0 * 100;
        return durationScore * 0.6 + qualityScore * 0.4;
    }
}
