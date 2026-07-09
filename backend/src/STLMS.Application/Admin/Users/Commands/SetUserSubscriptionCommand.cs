using FluentValidation.Results;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Commands;

/// <summary>Admin-only lever for subscription tier - there is no self-service upgrade path
/// (no payment integration) by design, so this is the only way a user's SubscriptionStatus ever
/// changes from its Free default.</summary>
public record SetUserSubscriptionCommand(Guid ActorUserId, Guid TargetUserId, string SubscriptionStatus, DateTime? ExpiresAt) : IRequest<bool>;

public class SetUserSubscriptionCommandHandler(IUnitOfWork uow, IAuditService auditService)
    : IRequestHandler<SetUserSubscriptionCommand, bool>
{
    public async Task<bool> HandleAsync(SetUserSubscriptionCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<SubscriptionStatus>(request.SubscriptionStatus, ignoreCase: true, out var status))
        {
            throw new ValidationException([new ValidationFailure(nameof(request.SubscriptionStatus), "Unknown subscription status.")]);
        }

        var user = await uow.Repository<User>().GetByIdAsync(request.TargetUserId, ct) ?? throw new NotFoundException("User", request.TargetUserId);
        var oldStatus = user.SubscriptionStatus;

        user.SubscriptionStatus = status;
        user.SubscriptionExpiresAt = request.ExpiresAt;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync(
            "SET_SUBSCRIPTION", "User", user.Id, new { SubscriptionStatus = oldStatus.ToString() },
            new { SubscriptionStatus = status.ToString(), request.ExpiresAt }, $"Set {user.Email}'s subscription to {status}", ct);

        return true;
    }
}
