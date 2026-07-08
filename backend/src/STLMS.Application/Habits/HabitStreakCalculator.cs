using STLMS.Domain.Entities;

namespace STLMS.Application.Habits;

/// <summary>Streaks are computed from HabitLog rows rather than stored redundantly - always
/// consistent with the underlying logs, at the cost of a bit of computation on read (cheap at
/// personal-habit-tracker scale). Days the habit isn't scheduled for (per RepeatDaysMask) are
/// skipped rather than breaking the streak - a rest day shouldn't reset progress.</summary>
public static class HabitStreakCalculator
{
    private const int MaxLookbackDays = 730;

    public static (int Current, int Longest) Calculate(int repeatDaysMask, IReadOnlyList<DateOnly> completedDates, DateOnly today)
    {
        bool IsScheduled(DateOnly d) => repeatDaysMask == 0 || (repeatDaysMask & AlarmDayMask.ForDayOfWeek(d.DayOfWeek)) != 0;

        var completedSet = completedDates.ToHashSet();

        // Current streak: if today is scheduled but not yet done, that's not a break - it just
        // doesn't count yet, so start looking from yesterday instead.
        var startDay = today;
        if (IsScheduled(today) && !completedSet.Contains(today)) startDay = today.AddDays(-1);

        var current = 0;
        var cursor = startDay;
        var oldestAllowed = today.AddDays(-MaxLookbackDays);
        while (cursor >= oldestAllowed)
        {
            if (IsScheduled(cursor))
            {
                if (completedSet.Contains(cursor)) current++;
                else break;
            }
            cursor = cursor.AddDays(-1);
        }

        var longest = 0;
        if (completedDates.Count > 0)
        {
            var running = 0;
            var earliest = completedDates.Min();
            if (earliest < oldestAllowed) earliest = oldestAllowed;
            for (var d = earliest; d <= today; d = d.AddDays(1))
            {
                if (!IsScheduled(d)) continue;
                if (completedSet.Contains(d))
                {
                    running++;
                    longest = Math.Max(longest, running);
                }
                else
                {
                    running = 0;
                }
            }
        }

        return (current, longest);
    }
}
