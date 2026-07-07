using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = default!; // e.g. CREATE, UPDATE, DELETE, LOGIN, LOGIN_FAILED
    public string EntityType { get; set; } = default!;
    public Guid? EntityId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
