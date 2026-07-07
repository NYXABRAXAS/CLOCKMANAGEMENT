using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool RememberMe { get; set; }
}

public class ExternalLogin : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public ExternalLoginProvider Provider { get; set; }
    public string ProviderUserId { get; set; } = default!;
}

public class UserSession : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string RefreshTokenHash { get; set; } = default!;
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public bool Revoked { get; set; }
}
