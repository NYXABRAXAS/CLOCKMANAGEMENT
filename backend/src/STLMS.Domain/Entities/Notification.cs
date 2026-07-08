using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>An in-app notification inbox entry. This is the only delivery channel wired up so far
/// (see INotificationDispatcher) - real push/email/SMS delivery lands in the Smart Notifications
/// milestone, which will dispatch through the same interface without any caller needing to change.</summary>
public class Notification : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
}
