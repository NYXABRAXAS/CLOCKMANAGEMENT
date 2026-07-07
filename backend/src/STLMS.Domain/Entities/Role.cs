using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

public class Role : AuditableEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>Seed-time constants for the 5 default roles. Roles themselves stay data-driven
/// (admin-editable via the Admin Panel) - these are just well-known codes to seed and to check
/// against in a handful of places (e.g. "is this user at least an Admin").</summary>
public static class RoleCodes
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string PremiumUser = "PREMIUM_USER";
    public const string StandardUser = "STANDARD_USER";
    public const string Guest = "GUEST";
}
