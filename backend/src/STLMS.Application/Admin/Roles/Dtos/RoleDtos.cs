namespace STLMS.Application.Admin.Roles.Dtos;

public record PermissionDto(Guid Id, string Module, string Action);

public record RoleDto(Guid Id, string Code, string Name, string? Description, bool IsSystem, IReadOnlyList<Guid> PermissionIds);
