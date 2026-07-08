using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

public class Medicine : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int RepeatDaysMask { get; set; } = AlarmDayMask.Everyday;
    public bool IsActive { get; set; } = true;

    public ICollection<MedicineTime> Times { get; set; } = new List<MedicineTime>();
}

/// <summary>One daily reminder time for a medicine (e.g. 8am and 8pm for a twice-daily dose).</summary>
public class MedicineTime : AuditableEntity
{
    public Guid MedicineId { get; set; }
    public Medicine Medicine { get; set; } = default!;
    public int Hour { get; set; }
    public int Minute { get; set; }
}

/// <summary>One recorded dose event - created only when the user actually marks a scheduled dose
/// taken or skipped (there's no background job pre-creating "pending" rows for every scheduled
/// time; "due now"/"overdue" is computed client-side from Medicine+MedicineTime against today's
/// existing logs).</summary>
public class MedicineLog : AuditableEntity
{
    public Guid MedicineId { get; set; }
    public Medicine Medicine { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public int ScheduledHour { get; set; }
    public int ScheduledMinute { get; set; }
    public MedicineLogStatus Status { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
