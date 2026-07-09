namespace STLMS.Application.Admin.Users.Dtos;

public record AdminUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified,
    bool IsActive,
    bool TwoFactorEnabled,
    string SubscriptionStatus,
    int FailedLoginAttempts,
    DateTime? LockedUntil,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles);
