using STLMS.Application.Admin.Roles.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Roles.Queries;

public record GetPermissionsQuery : IRequest<IReadOnlyList<PermissionDto>>;

public class GetPermissionsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> HandleAsync(GetPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await uow.Repository<Permission>().GetAllAsync(ct);
        return permissions
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .Select(p => new PermissionDto(p.Id, p.Module, p.Action))
            .ToList();
    }
}
