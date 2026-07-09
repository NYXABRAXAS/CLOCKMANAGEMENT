using STLMS.Application.Productivity;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using Xunit;

namespace STLMS.Application.Tests.Productivity;

public class ProductivityScoreCalculatorTests
{
    private static readonly DateOnly Date = new(2026, 7, 9);

    [Fact]
    public void ScoreDay_NoApplicableComponents_ReturnsNullScoreNotZero()
    {
        // Regression test for a real bug: a day with zero applicable components (nothing scheduled,
        // no module adopted yet) must be null, not a misleading 0 that tanks the historical average.
        var result = ProductivityScoreCalculator.ScoreDay(
            Date,
            habitsPercent: null,
            medicinesPercent: null,
            sleepLog: null,
            focusMinutes: 0,
            sleepModuleUsed: false,
            pomodoroModuleUsed: false,
            prayersApplicable: false,
            prayersPercent: null);

        Assert.Null(result.Score);
    }

    [Fact]
    public void ScoreDay_AveragesOnlyApplicableComponents()
    {
        // Habits 100%, medicines 100%, no sleep/pomodoro/prayers adopted - average must be over
        // exactly those two components, not diluted by inapplicable ones counted as 0.
        var result = ProductivityScoreCalculator.ScoreDay(
            Date,
            habitsPercent: 100,
            medicinesPercent: 100,
            sleepLog: null,
            focusMinutes: 0,
            sleepModuleUsed: false,
            pomodoroModuleUsed: false,
            prayersApplicable: false,
            prayersPercent: null);

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void ScoreDay_PomodoroModuleUsed_ScoresZeroFocusMinutesAsZeroNotNull()
    {
        // Unlike habits/medicines, pomodoro is applicable every day once adopted - 0 minutes that
        // day is a real, honest 0 (no focused work happened), not a gap.
        var result = ProductivityScoreCalculator.ScoreDay(
            Date,
            habitsPercent: null,
            medicinesPercent: null,
            sleepLog: null,
            focusMinutes: 0,
            sleepModuleUsed: false,
            pomodoroModuleUsed: true,
            prayersApplicable: false,
            prayersPercent: null);

        Assert.Equal(0, result.Score);
    }

    [Theory]
    [InlineData(45, 50)] // half of the 90-minute target
    [InlineData(90, 100)] // exactly at target - full score
    [InlineData(180, 100)] // well past target - capped at 100, never exceeds it
    public void ScoreDay_PomodoroScore_ScalesLinearlyUpToNinetyMinutesThenCaps(int focusMinutes, double expectedScore)
    {
        var result = ProductivityScoreCalculator.ScoreDay(
            Date, null, null, null, focusMinutes, sleepModuleUsed: false, pomodoroModuleUsed: true, prayersApplicable: false, null);

        Assert.Equal(expectedScore, result.Score);
    }

    [Fact]
    public void ScoreDay_SleepLogNull_ButModuleAdopted_ScoresZero()
    {
        var result = ProductivityScoreCalculator.ScoreDay(
            Date, null, null, sleepLog: null, focusMinutes: 0, sleepModuleUsed: true, pomodoroModuleUsed: false, prayersApplicable: false, null);

        Assert.Equal(0, result.Components.SleepScore);
    }

    [Fact]
    public void ScoreDay_SleepLogAtExactTargetWithGoodQuality_BlendsDurationAndQualityScores()
    {
        // 8 hours exactly (0 minutes off target) => durationScore = 100.
        // Quality = Good (index 2 of 4) => qualityScore = 2/3 * 100 = 66.667.
        // Blend: 100 * 0.6 + 66.667 * 0.4 = 86.667.
        var sleepLog = new SleepLog { DurationMinutes = 480, Quality = SleepQuality.Good };

        var result = ProductivityScoreCalculator.ScoreDay(
            Date, null, null, sleepLog, focusMinutes: 0, sleepModuleUsed: true, pomodoroModuleUsed: false, prayersApplicable: false, null);

        Assert.NotNull(result.Components.SleepScore);
        Assert.Equal(86.667, result.Components.SleepScore!.Value, precision: 2);
    }

    [Fact]
    public void ScoreDay_SleepLogWithNoQualityRated_UsesDurationScoreOnly()
    {
        var sleepLog = new SleepLog { DurationMinutes = 480, Quality = null };

        var result = ProductivityScoreCalculator.ScoreDay(
            Date, null, null, sleepLog, focusMinutes: 0, sleepModuleUsed: true, pomodoroModuleUsed: false, prayersApplicable: false, null);

        Assert.Equal(100, result.Components.SleepScore);
    }

    [Fact]
    public void ScoreDay_SleepFarFromTarget_ScoresLow()
    {
        // 3 hours (180 min) away from the 480-minute target => durationScore floors at 0.
        var sleepLog = new SleepLog { DurationMinutes = 300, Quality = null };

        var result = ProductivityScoreCalculator.ScoreDay(
            Date, null, null, sleepLog, focusMinutes: 0, sleepModuleUsed: true, pomodoroModuleUsed: false, prayersApplicable: false, null);

        Assert.Equal(0, result.Components.SleepScore);
    }

    [Theory]
    [InlineData(0b1111111, DayOfWeek.Wednesday, true)] // Everyday mask - always scheduled
    [InlineData(0, DayOfWeek.Wednesday, true)] // 0 mask means "every day" too, per the shared convention
    public void IsScheduled_EverydayOrZeroMask_AlwaysTrue(int mask, DayOfWeek day, bool expected)
    {
        Assert.Equal(expected, ProductivityScoreCalculator.IsScheduled(mask, day));
    }

    [Fact]
    public void IsScheduled_SpecificDayMask_OnlyMatchesThatDay()
    {
        var mondayOnly = AlarmDayMask.Monday;

        Assert.True(ProductivityScoreCalculator.IsScheduled(mondayOnly, DayOfWeek.Monday));
        Assert.False(ProductivityScoreCalculator.IsScheduled(mondayOnly, DayOfWeek.Tuesday));
    }
}
