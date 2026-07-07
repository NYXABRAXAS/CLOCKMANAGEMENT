using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A single module:action pair (e.g. "ALARMS":"create"). Mirrors the generic
/// module/action permission catalog pattern - new modules/actions are just new rows, never
/// require a code change.</summary>
public class Permission : AuditableEntity
{
    public string Module { get; set; } = default!;
    public string Action { get; set; } = default!;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
}
