using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.Services;

public class NotificationDispatcher(IUnitOfWork uow) : INotificationDispatcher
{
    public async Task DispatchAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        await uow.Repository<Notification>().AddAsync(new Notification { UserId = userId, Title = title, Message = message }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
