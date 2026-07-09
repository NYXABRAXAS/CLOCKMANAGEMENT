using STLMS.Application.Admin.Roles.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Roles.Queries;

public record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public class GetRolesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    public async Task<IReadOnlyList<RoleDto>> HandleAsync(GetRolesQuery request, CancellationToken ct)
    {
        var roles = await uow.Repository<Role>().GetAllAsync(ct);
        var rolePermissions = await uow.Repository<RolePermission>().GetAllAsync(ct);
        var permissionsByRole = rolePermissions.GroupBy(rp => rp.RoleId).ToDictionary(g => g.Key, g => g.Select(rp => rp.PermissionId).ToList());

        return roles
            .OrderBy(r => r.SortOrder)
            .Select(r => new RoleDto(r.Id, r.Code, r.Name, r.Description, r.IsSystem, permissionsByRole.GetValueOrDefault(r.Id, [])))
            .ToList();
    }
}
