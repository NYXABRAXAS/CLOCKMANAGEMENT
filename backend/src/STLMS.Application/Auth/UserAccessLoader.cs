using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth;

/// <summary>Flattens a user's roles into role codes and role-permissions into "MODULE:action"
/// strings. Plain LINQ over IRepository&lt;T&gt;.Query() (no EF Core Include()). Deliberately
/// synchronous ToList() rather than EF Core's ToListAsync(): EF's async LINQ extensions require
/// the queryable to come from a genuine EF query provider (they throw against a plain
/// LINQ-to-Objects IQueryable, which is exactly what test doubles use) - confirmed by a failing
/// unit test against FakeRepository. This query is a handful of role/permission rows via primary
/// key lookups, so the blocking cost is negligible next to the testability this buys.</summary>
public static class UserAccessLoader
{
    public static Task<(IReadOnlyList<string> roles, IReadOnlyList<string> permissions)> LoadAsync(IUnitOfWork uow, Guid userId, CancellationToken ct)
    {
        var roleIds = uow.Repository<UserRole>().Query().Where(ur => ur.UserId == userId).Select(ur => ur.RoleId);

        var roles = uow.Repository<Role>().Query().Where(r => roleIds.Contains(r.Id)).Select(r => r.Code).ToList();

        var permissionIds = uow.Repository<RolePermission>().Query()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct();

        var permissions = uow.Repository<Permission>().Query()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Module + ":" + p.Action)
            .ToList();

        return Task.FromResult<(IReadOnlyList<string>, IReadOnlyList<string>)>((roles, permissions));
    }
}
