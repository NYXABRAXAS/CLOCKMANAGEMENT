using Microsoft.Extensions.Logging;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.Services;

/// <summary>Single fan-out point every reminder module dispatches through. Always persists the
/// in-app Notification row (the one channel guaranteed to work with zero configuration), then
/// best-effort fans out to email/push per the user's own toggles - failures on those channels are
/// logged, never thrown, so a bad SMTP/Firebase config can never block the in-app notification
/// (which is the one thing every caller actually depends on succeeding).</summary>
public class NotificationDispatcher(IUnitOfWork uow, IEmailSender emailSender, IPushSender pushSender, ILogger<NotificationDispatcher> logger)
    : INotificationDispatcher
{
    public async Task DispatchAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        await uow.Repository<Notification>().AddAsync(new Notification { UserId = userId, Title = title, Message = message }, ct);
        await uow.SaveChangesAsync(ct);

        var user = await uow.Repository<User>().GetByIdAsync(userId, ct);
        if (user is null) return;

        if (user.EmailNotificationsEnabled && user.EmailVerified)
        {
            try
            {
                await emailSender.SendAsync(user.Email, title, $"<p>{message}</p>", ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email fan-out failed for notification \"{Title}\" to user {UserId}", title, userId);
            }
        }

        if (user.PushNotificationsEnabled)
        {
            try
            {
                await pushSender.SendToUserAsync(userId, title, message, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Push fan-out failed for notification \"{Title}\" to user {UserId}", title, userId);
            }
        }
    }
}
