using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

/// <summary>A calendar event, optionally recurring. Recurrence is a simple frequency+interval+
/// weekday-mask model (reusing the same day-mask idea as Alarm) rather than full RFC5545 RRULE
/// parsing - enough to expand real occurrences without a parsing library.
/// ExternalProvider/ExternalEventId are schema-ready for a future Google/Outlook sync milestone -
/// unused until that's built, per the plan's "architecture only, no live sync" scope here.</summary>
public class CalendarEvent : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public RecurrenceFrequency RecurrenceFrequency { get; set; } = RecurrenceFrequency.None;
    public int RecurrenceInterval { get; set; } = 1; // every N days/weeks/months/years
    public int RecurrenceDaysMask { get; set; } // for Weekly: reuses AlarmDayMask bit values
    public DateTime? RecurrenceEndDate { get; set; } // null = repeats indefinitely

    public string? ExternalProvider { get; set; }
    public string? ExternalEventId { get; set; }
}
