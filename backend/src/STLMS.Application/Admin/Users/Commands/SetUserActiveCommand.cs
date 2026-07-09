using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Commands;

public record SetUserActiveCommand(Guid ActorUserId, Guid TargetUserId, bool IsActive) : IRequest<bool>;

public class SetUserActiveCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<SetUserActiveCommand, bool>
{
    public async Task<bool> HandleAsync(SetUserActiveCommand request, CancellationToken ct)
    {
        if (request.ActorUserId == request.TargetUserId)
        {
            throw new ConflictException("You can't deactivate your own account.");
        }

        var user = await uow.Repository<User>().GetByIdAsync(request.TargetUserId, ct) ?? throw new NotFoundException("User", request.TargetUserId);
        var wasActive = user.IsActive;
        user.IsActive = request.IsActive;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync(
            request.IsActive ? "ACTIVATE" : "DEACTIVATE", "User", user.Id, new { IsActive = wasActive }, new { user.IsActive }, ct: ct);

        return true;
    }
}
