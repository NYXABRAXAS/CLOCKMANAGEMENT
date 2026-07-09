using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Roles.Commands;

public record SetRolePermissionCommand(Guid ActorUserId, Guid RoleId, Guid PermissionId, bool Granted) : IRequest<bool>;

public class SetRolePermissionCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<SetRolePermissionCommand, bool>
{
    public async Task<bool> HandleAsync(SetRolePermissionCommand request, CancellationToken ct)
    {
        var role = await uow.Repository<Role>().GetByIdAsync(request.RoleId, ct) ?? throw new NotFoundException("Role", request.RoleId);
        if (role.Code == RoleCodes.SuperAdmin)
        {
            throw new ConflictException("Super Admin's permissions can't be changed - it must always retain full access.");
        }

        var permission = await uow.Repository<Permission>().GetByIdAsync(request.PermissionId, ct)
            ?? throw new NotFoundException("Permission", request.PermissionId);

        var existing = await uow.Repository<RolePermission>().SingleOrDefaultAsync(
            rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, ct);

        if (request.Granted && existing is null)
        {
            await uow.Repository<RolePermission>().AddAsync(new RolePermission { RoleId = request.RoleId, PermissionId = request.PermissionId }, ct);
        }
        else if (!request.Granted && existing is not null)
        {
            uow.Repository<RolePermission>().Remove(existing);
        }
        else
        {
            return true;
        }

        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync(
            request.Granted ? "GRANT_PERMISSION" : "REVOKE_PERMISSION", "Role", role.Id,
            description: $"{(request.Granted ? "Granted" : "Revoked")} {permission.Module}:{permission.Action} {(request.Granted ? "to" : "from")} {role.Code}",
            ct: ct);

        return true;
    }
}
