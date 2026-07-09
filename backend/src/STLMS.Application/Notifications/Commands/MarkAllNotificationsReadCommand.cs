using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Notifications.Commands;

public record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<bool>;

public class MarkAllNotificationsReadCommandHandler(IUnitOfWork uow) : IRequestHandler<MarkAllNotificationsReadCommand, bool>
{
    public async Task<bool> HandleAsync(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var unread = await uow.Repository<Notification>().FindAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            uow.Repository<Notification>().Update(notification);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
