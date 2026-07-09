namespace STLMS.Application.Common.Interfaces;

/// <summary>Sends a push notification to every device the user has registered. Same "return
/// false, never throw" contract as IEmailSender - a missing/invalid Firebase configuration or an
/// unregistered device token must never block the caller (NotificationDispatcher) from completing
/// its other delivery channels.</summary>
public interface IPushSender
{
    Task<bool> SendToUserAsync(Guid userId, string title, string message, CancellationToken ct = default);
}
