using FluentValidation.Results;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Commands;

/// <summary>Replaces the target user's role assignment with a single new role. UserRole is
/// technically a many-to-many join, but every place a role is assigned in this app (registration,
/// this command) treats "a user's role" as singular - simplest model that matches actual usage,
/// not a limitation of the schema.</summary>
public record AssignUserRoleCommand(Guid ActorUserId, Guid TargetUserId, string RoleCode) : IRequest<bool>;

public class AssignUserRoleCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<AssignUserRoleCommand, bool>
{
    public async Task<bool> HandleAsync(AssignUserRoleCommand request, CancellationToken ct)
    {
        if (request.ActorUserId == request.TargetUserId)
        {
            throw new ConflictException("You can't change your own role.");
        }

        var user = await uow.Repository<User>().GetByIdAsync(request.TargetUserId, ct) ?? throw new NotFoundException("User", request.TargetUserId);
        var role = await uow.Repository<Role>().SingleOrDefaultAsync(r => r.Code == request.RoleCode, ct)
            ?? throw new ValidationException([new ValidationFailure(nameof(request.RoleCode), "Unknown role code.")]);

        var existingAssignments = await uow.Repository<UserRole>().FindAsync(ur => ur.UserId == user.Id, ct);
        var previousRoleCodes = string.Join(",", existingAssignments.Select(a => a.RoleId));
        foreach (var assignment in existingAssignments)
        {
            uow.Repository<UserRole>().Remove(assignment);
        }

        await uow.Repository<UserRole>().AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id }, ct);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync(
            "ASSIGN_ROLE", "User", user.Id, new { PreviousRoleIds = previousRoleCodes }, new { NewRoleCode = role.Code },
            $"Assigned role {role.Code} to {user.Email}", ct);

        return true;
    }
}
