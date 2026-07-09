using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Notifications.Commands;

public record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<bool>;

public class MarkNotificationReadCommandHandler(IUnitOfWork uow) : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> HandleAsync(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await uow.Repository<Notification>().GetByIdAsync(request.NotificationId, ct);
        if (notification is null || notification.UserId != request.UserId) throw new NotFoundException("Notification", request.NotificationId);

        notification.IsRead = true;
        uow.Repository<Notification>().Update(notification);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
