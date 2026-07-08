using STLMS.Domain.Entities;
using STLMS.Domain.Enums;

namespace STLMS.Application.CalendarEvents;

/// <summary>Expands a (possibly recurring) CalendarEvent into concrete occurrence start/end
/// instants that fall within [rangeStart, rangeEnd). Deliberately a simple frequency+interval+
/// weekday-mask model rather than full RFC5545 RRULE parsing - the plan calls this out as
/// "architecture only" ahead of a real external-calendar-sync milestone, so this only needs to be
/// correct, not standards-compliant.</summary>
public static class CalendarRecurrenceExpander
{
    private const int MaxOccurrences = 500; // guards against unbounded loops for huge ranges

    public static IEnumerable<(DateTime Start, DateTime End)> Expand(CalendarEvent e, DateTime rangeStart, DateTime rangeEnd)
    {
        var duration = e.EndAt - e.StartAt;
        var seriesEnd = e.RecurrenceEndDate ?? rangeEnd;
        var effectiveEnd = seriesEnd < rangeEnd ? seriesEnd : rangeEnd;

        if (e.RecurrenceFrequency == RecurrenceFrequency.None)
        {
            if (e.StartAt < rangeEnd && e.EndAt > rangeStart) yield return (e.StartAt, e.EndAt);
            yield break;
        }

        var count = 0;
        foreach (var occurrenceStart in EnumerateOccurrenceStarts(e, rangeStart, effectiveEnd))
        {
            if (++count > MaxOccurrences) yield break;
            var occurrenceEnd = occurrenceStart + duration;
            if (occurrenceStart < rangeEnd && occurrenceEnd > rangeStart) yield return (occurrenceStart, occurrenceEnd);
        }
    }

    private static IEnumerable<DateTime> EnumerateOccurrenceStarts(CalendarEvent e, DateTime rangeStart, DateTime effectiveEnd)
    {
        switch (e.RecurrenceFrequency)
        {
            case RecurrenceFrequency.Daily:
            {
                var cursor = e.StartAt;
                while (cursor < effectiveEnd)
                {
                    yield return cursor; // Expand() filters by rangeStart/rangeEnd against the full occurrence span
                    cursor = cursor.AddDays(Math.Max(1, e.RecurrenceInterval));
                }
                break;
            }
            case RecurrenceFrequency.Weekly:
            {
                var daysMask = e.RecurrenceDaysMask != 0 ? e.RecurrenceDaysMask : AlarmDayMask.ForDayOfWeek(e.StartAt.DayOfWeek);
                var weekStart = e.StartAt.Date.AddDays(-(int)e.StartAt.DayOfWeek); // Sunday of the series' first week
                var weekCursor = weekStart;
                while (weekCursor < effectiveEnd)
                {
                    for (var i = 0; i < 7; i++)
                    {
                        var day = weekCursor.AddDays(i);
                        if (day < e.StartAt.Date) continue;
                        if ((daysMask & AlarmDayMask.ForDayOfWeek(day.DayOfWeek)) == 0) continue;
                        yield return day.Add(e.StartAt.TimeOfDay);
                    }
                    weekCursor = weekCursor.AddDays(7 * Math.Max(1, e.RecurrenceInterval));
                }
                break;
            }
            case RecurrenceFrequency.Monthly:
            {
                var cursor = e.StartAt;
                while (cursor < effectiveEnd)
                {
                    yield return cursor;
                    cursor = cursor.AddMonths(Math.Max(1, e.RecurrenceInterval));
                }
                break;
            }
            case RecurrenceFrequency.Yearly:
            {
                var cursor = e.StartAt;
                while (cursor < effectiveEnd)
                {
                    yield return cursor;
                    cursor = cursor.AddYears(Math.Max(1, e.RecurrenceInterval));
                }
                break;
            }
        }
    }
}
