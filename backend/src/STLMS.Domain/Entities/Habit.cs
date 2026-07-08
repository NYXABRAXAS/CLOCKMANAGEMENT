using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

public class Habit : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public string? Color { get; set; }
    public int RepeatDaysMask { get; set; } = AlarmDayMask.Everyday;
    public bool IsActive { get; set; } = true;

    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
}

/// <summary>One check-in per calendar day. Current/longest streak are computed on read from these
/// rows rather than stored redundantly - a derived value that's always consistent beats a cached
/// one that can drift out of sync with the logs it's supposed to summarize.</summary>
public class HabitLog : AuditableEntity
{
    public Guid HabitId { get; set; }
    public Habit Habit { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public bool Completed { get; set; }
}
