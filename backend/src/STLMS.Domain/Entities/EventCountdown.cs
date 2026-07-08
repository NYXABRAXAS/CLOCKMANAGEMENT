using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A lightweight "X days until Y" countdown to a target date - distinct from
/// CalendarEvent (no time-of-day, no recurrence, just a milestone date to count down to, e.g. a
/// birthday or trip).</summary>
public class EventCountdown : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Title { get; set; } = default!;
    public DateOnly TargetDate { get; set; }
    public string? Emoji { get; set; }
    public string? Color { get; set; }
}
