using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A city a user has pinned to their World Clock widget/page.</summary>
public class WorldClockCity : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid CityId { get; set; }
    public City City { get; set; } = default!;
    public int SortOrder { get; set; }
}
