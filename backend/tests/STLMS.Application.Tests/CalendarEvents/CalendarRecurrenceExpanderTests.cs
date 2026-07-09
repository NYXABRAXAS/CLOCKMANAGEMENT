using STLMS.Application.CalendarEvents;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using Xunit;

namespace STLMS.Application.Tests.CalendarEvents;

public class CalendarRecurrenceExpanderTests
{
    private static CalendarEvent MakeEvent(DateTime startAt, DateTime endAt, RecurrenceFrequency frequency = RecurrenceFrequency.None) =>
        new() { StartAt = startAt, EndAt = endAt, RecurrenceFrequency = frequency };

    [Fact]
    public void Expand_NonRecurringEventWithinRange_ReturnsSingleOccurrence()
    {
        var e = MakeEvent(new DateTime(2026, 7, 10, 9, 0, 0), new DateTime(2026, 7, 10, 10, 0, 0));

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 7, 1), new DateTime(2026, 8, 1)).ToList();

        Assert.Single(occurrences);
        Assert.Equal(e.StartAt, occurrences[0].Start);
        Assert.Equal(e.EndAt, occurrences[0].End);
    }

    [Fact]
    public void Expand_NonRecurringEventOutsideRange_ReturnsNothing()
    {
        var e = MakeEvent(new DateTime(2026, 7, 10, 9, 0, 0), new DateTime(2026, 7, 10, 10, 0, 0));

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 9, 1), new DateTime(2026, 10, 1)).ToList();

        Assert.Empty(occurrences);
    }

    [Fact]
    public void Expand_Daily_ProducesOneOccurrencePerDayWithinRange()
    {
        var e = MakeEvent(new DateTime(2026, 7, 1, 8, 0, 0), new DateTime(2026, 7, 1, 8, 30, 0), RecurrenceFrequency.Daily);
        e.RecurrenceInterval = 1;

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 7, 1), new DateTime(2026, 7, 8)).ToList();

        Assert.Equal(7, occurrences.Count);
        Assert.All(occurrences, o => Assert.Equal(TimeSpan.FromMinutes(30), o.End - o.Start));
    }

    [Fact]
    public void Expand_DailyWithInterval_SkipsAccordingToInterval()
    {
        var e = MakeEvent(new DateTime(2026, 7, 1, 8, 0, 0), new DateTime(2026, 7, 1, 8, 30, 0), RecurrenceFrequency.Daily);
        e.RecurrenceInterval = 2; // every other day

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 7, 1), new DateTime(2026, 7, 8)).ToList();

        // July 1, 3, 5, 7 within [July 1, July 8)
        Assert.Equal(4, occurrences.Count);
    }

    [Fact]
    public void Expand_WeeklyWithDaysMask_OnlyMatchesScheduledWeekdays()
    {
        // A weekly event scheduled for Mon/Wed/Fri should produce exactly 3 occurrences per week.
        var mondayStart = new DateTime(2026, 7, 6, 18, 0, 0); // 2026-07-06 is a Monday
        Assert.Equal(DayOfWeek.Monday, mondayStart.DayOfWeek);

        var e = MakeEvent(mondayStart, mondayStart.AddHours(1), RecurrenceFrequency.Weekly);
        e.RecurrenceInterval = 1;
        e.RecurrenceDaysMask = AlarmDayMask.Monday | AlarmDayMask.Wednesday | AlarmDayMask.Friday;

        // Two full weeks starting from the series' first Monday.
        var occurrences = CalendarRecurrenceExpander.Expand(e, mondayStart, mondayStart.AddDays(14)).ToList();

        Assert.Equal(6, occurrences.Count);
        Assert.All(occurrences, o => Assert.Contains(o.Start.DayOfWeek, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }));
    }

    [Fact]
    public void Expand_RecurrenceEndDate_StopsProducingOccurrencesAfterIt()
    {
        var e = MakeEvent(new DateTime(2026, 7, 1, 8, 0, 0), new DateTime(2026, 7, 1, 8, 30, 0), RecurrenceFrequency.Daily);
        e.RecurrenceInterval = 1;
        e.RecurrenceEndDate = new DateTime(2026, 7, 4); // series ends before the requested range does

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 7, 1), new DateTime(2026, 7, 31)).ToList();

        Assert.Equal(3, occurrences.Count); // July 1, 2, 3 - stops before RecurrenceEndDate
    }

    [Fact]
    public void Expand_HugeRangeWithTinyInterval_IsCappedRatherThanHanging()
    {
        var e = MakeEvent(new DateTime(2000, 1, 1), new DateTime(2000, 1, 1, 0, 30, 0), RecurrenceFrequency.Daily);
        e.RecurrenceInterval = 1;

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2000, 1, 1), new DateTime(2100, 1, 1)).ToList();

        Assert.True(occurrences.Count <= 500);
    }

    [Fact]
    public void Expand_Monthly_AdvancesByCalendarMonthNotFixedDayCount()
    {
        var e = MakeEvent(new DateTime(2026, 1, 31, 9, 0, 0), new DateTime(2026, 1, 31, 10, 0, 0), RecurrenceFrequency.Monthly);
        e.RecurrenceInterval = 1;

        var occurrences = CalendarRecurrenceExpander.Expand(e, new DateTime(2026, 1, 1), new DateTime(2026, 4, 1)).ToList();

        // Jan 31 -> Feb 28 (AddMonths clamps to the shorter month) -> Mar 28 (the clamp "sticks":
        // AddMonths operates on the already-clamped cursor, not the original day-of-month, so it
        // drifts to the 28th rather than reverting to the 31st once March allows it).
        Assert.Equal(3, occurrences.Count);
        Assert.Equal(new DateTime(2026, 1, 31, 9, 0, 0), occurrences[0].Start);
        Assert.Equal(new DateTime(2026, 2, 28, 9, 0, 0), occurrences[1].Start);
        Assert.Equal(new DateTime(2026, 3, 28, 9, 0, 0), occurrences[2].Start);
    }
}
