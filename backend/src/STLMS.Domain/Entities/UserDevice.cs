using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>An FCM registration token for one browser/device the user has granted push permission
/// on. A user can have several (multiple browsers/devices); push fan-out sends to all of them and
/// prunes any Firebase reports as no-longer-registered.</summary>
public class UserDevice : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string FcmToken { get; set; } = default!;
    public string? Platform { get; set; } // "web" | "android" | "ios" - web is the only one this app issues today
}
