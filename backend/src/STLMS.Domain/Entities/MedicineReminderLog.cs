using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>One row per reminder notification actually sent for a scheduled dose time - the
/// dedupe record MedicineReminderService checks before firing again, playing the same role
/// AlarmHistory plays for AlarmTriggerService. Deliberately separate from MedicineLog, which
/// records the user's own taken/skipped action rather than "a reminder was sent".</summary>
public class MedicineReminderLog : AuditableEntity
{
    public Guid MedicineId { get; set; }
    public Medicine Medicine { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
