using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = default!;
    public bool EmailVerified { get; set; }
    public string? PasswordHash { get; set; } // null for accounts created purely via external login
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? PhotoUrl { get; set; }

    public string? CountryCode { get; set; }
    public string TimezoneId { get; set; } = "UTC"; // IANA id, e.g. "Asia/Kolkata"
    public string TimeFormat { get; set; } = "24h"; // "12h" | "24h"
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "system"; // "light" | "dark" | "system"

    public Guid? ReligionId { get; set; }
    public Religion? Religion { get; set; }
    public double? PrayerLatitude { get; set; }
    public double? PrayerLongitude { get; set; }

    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool TwoFactorEnabled { get; set; }
    public string? TotpSecretEncrypted { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationExpiresAt { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}

public class UserRole : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
}
