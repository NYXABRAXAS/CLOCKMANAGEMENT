using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Notifications.Queries;

public record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler(IUnitOfWork uow) : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> HandleAsync(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var unread = await uow.Repository<Notification>().FindAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
        return unread.Count;
    }
}
