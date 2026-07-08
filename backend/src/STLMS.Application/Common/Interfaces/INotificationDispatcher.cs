namespace STLMS.Application.Common.Interfaces;

/// <summary>Single entry point every reminder module (Alarms, and later Medicine/Habit/Calendar
/// reminders) dispatches through - the concrete implementation decides where notifications
/// actually go (today: an in-app Notification row; later: also push/email/SMS) without callers
/// needing to know or change.</summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(Guid userId, string title, string message, CancellationToken ct = default);
}
