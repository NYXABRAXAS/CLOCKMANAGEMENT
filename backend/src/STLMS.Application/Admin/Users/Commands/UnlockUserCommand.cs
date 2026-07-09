using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Commands;

public record UnlockUserCommand(Guid ActorUserId, Guid TargetUserId) : IRequest<bool>;

public class UnlockUserCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<UnlockUserCommand, bool>
{
    public async Task<bool> HandleAsync(UnlockUserCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.TargetUserId, ct) ?? throw new NotFoundException("User", request.TargetUserId);

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("UNLOCK", "User", user.Id, description: $"Unlocked {user.Email}", ct: ct);

        return true;
    }
}
