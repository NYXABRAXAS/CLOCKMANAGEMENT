using STLMS.Application.Habits;
using STLMS.Domain.Entities;
using Xunit;

namespace STLMS.Application.Tests.Habits;

public class HabitStreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 7, 9);

    [Fact]
    public void Calculate_NoLogs_ReturnsZero()
    {
        var (current, longest) = HabitStreakCalculator.Calculate(AlarmDayMask.Everyday, [], Today);

        Assert.Equal(0, current);
        Assert.Equal(0, longest);
    }

    [Fact]
    public void Calculate_ConsecutiveDaysEndingToday_CountsCurrentStreak()
    {
        DateOnly[] completed = [Today.AddDays(-2), Today.AddDays(-1), Today];

        var (current, longest) = HabitStreakCalculator.Calculate(AlarmDayMask.Everyday, completed, Today);

        Assert.Equal(3, current);
        Assert.Equal(3, longest);
    }

    [Fact]
    public void Calculate_TodayNotYetCompleted_DoesNotBreakStreak()
    {
        // Today is scheduled but has no log yet - that's "not done yet", not a break, so the
        // streak should still count from yesterday backward.
        DateOnly[] completed = [Today.AddDays(-2), Today.AddDays(-1)];

        var (current, _) = HabitStreakCalculator.Calculate(AlarmDayMask.Everyday, completed, Today);

        Assert.Equal(2, current);
    }

    [Fact]
    public void Calculate_MissedScheduledDay_BreaksCurrentStreak()
    {
        // Yesterday was scheduled and missed - the streak resets to just today.
        DateOnly[] completed = [Today.AddDays(-3), Today];

        var (current, _) = HabitStreakCalculator.Calculate(AlarmDayMask.Everyday, completed, Today);

        Assert.Equal(1, current);
    }

    [Fact]
    public void Calculate_RestDayNotScheduled_IsSkippedRatherThanBreakingStreak()
    {
        // Exclude whatever weekday falls immediately before "today" from the schedule - that day
        // has no log, but since it was never scheduled, it must not break the streak.
        var restDay = Today.AddDays(-1);
        var mask = AlarmDayMask.Everyday & ~AlarmDayMask.ForDayOfWeek(restDay.DayOfWeek);
        DateOnly[] completed = [Today.AddDays(-2), Today];

        var (current, _) = HabitStreakCalculator.Calculate(mask, completed, Today);

        Assert.Equal(2, current);
    }

    [Fact]
    public void Calculate_LongestStreakIsIndependentOfCurrentStreak()
    {
        // A 5-day streak two weeks ago, then a gap, then today is day 1 of a fresh streak.
        DateOnly[] completed =
        [
            Today.AddDays(-20), Today.AddDays(-19), Today.AddDays(-18), Today.AddDays(-17), Today.AddDays(-16),
            Today,
        ];

        var (current, longest) = HabitStreakCalculator.Calculate(AlarmDayMask.Everyday, completed, Today);

        Assert.Equal(1, current);
        Assert.Equal(5, longest);
    }
}
