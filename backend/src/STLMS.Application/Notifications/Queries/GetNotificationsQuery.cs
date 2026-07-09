using STLMS.Application.Common.Mediator;
using STLMS.Application.Notifications.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Notifications.Queries;

public record GetNotificationsQuery(Guid UserId) : IRequest<IReadOnlyList<NotificationDto>>;

public class GetNotificationsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public async Task<IReadOnlyList<NotificationDto>> HandleAsync(GetNotificationsQuery request, CancellationToken ct)
    {
        var notifications = await uow.Repository<Notification>().FindAsync(n => n.UserId == request.UserId, ct);
        return notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt))
            .ToList();
    }
}
