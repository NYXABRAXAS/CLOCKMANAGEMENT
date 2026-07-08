using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A completed stopwatch run, saved once the user stops it (the live ticking itself is
/// client-side).</summary>
public class StopwatchSession : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Label { get; set; } = default!;
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public long TotalDurationMs { get; set; }

    public ICollection<StopwatchLap> Laps { get; set; } = new List<StopwatchLap>();
}

public class StopwatchLap : AuditableEntity
{
    public Guid StopwatchSessionId { get; set; }
    public StopwatchSession StopwatchSession { get; set; } = default!;
    public int LapNumber { get; set; }
    public long LapDurationMs { get; set; }
    public long CumulativeDurationMs { get; set; }
}
