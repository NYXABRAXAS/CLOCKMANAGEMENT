using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

public class SleepLog : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public DateOnly Date { get; set; } // the night this log is for (the date the user woke up)
    public DateTime BedTime { get; set; }
    public DateTime WakeTime { get; set; }
    public int DurationMinutes { get; set; }
    public SleepQuality? Quality { get; set; }
    public string? Notes { get; set; }
}
