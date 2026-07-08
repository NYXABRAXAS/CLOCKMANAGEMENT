using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

/// <summary>One run of the Pomodoro technique with its own work/break configuration (users can
/// change these between runs). Ends when the user stops it or completes the planned cycles.</summary>
public class PomodoroSession : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public int WorkMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int CyclesBeforeLongBreak { get; set; } = 4;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public ICollection<PomodoroLog> Logs { get; set; } = new List<PomodoroLog>();
}

/// <summary>One completed work/break phase within a session - the raw material Milestone 11's
/// Productivity Dashboard aggregates into "focused minutes today" style metrics.</summary>
public class PomodoroLog : AuditableEntity
{
    public Guid PomodoroSessionId { get; set; }
    public PomodoroSession PomodoroSession { get; set; } = default!;
    public PomodoroPhase Phase { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public bool CompletedFully { get; set; } // false if the user skipped/interrupted the phase early
}
