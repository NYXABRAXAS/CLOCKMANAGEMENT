using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A saved, reusable timer preset (e.g. "Tea - 5 min") a user can quick-launch. The
/// actual countdown ticking happens client-side - this is just the saved definition.</summary>
public class CountdownTimer : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Label { get; set; } = default!;
    public int DurationSeconds { get; set; }
    public string SoundId { get; set; } = "classic";
}
