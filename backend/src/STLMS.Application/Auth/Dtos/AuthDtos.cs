namespace STLMS.Application.Auth.Dtos;

public record UserProfileDto(
    Guid Id,
    string Email,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? PhotoUrl,
    string? ReligionCode,
    string? CountryCode,
    string TimezoneId,
    string TimeFormat,
    string Language,
    string Theme,
    double? PrayerLatitude,
    double? PrayerLongitude,
    int? PrayerCalculationMethod,
    string SubscriptionStatus,
    bool TwoFactorEnabled,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

/// <summary>What a successful login/register/refresh/external-login returns to the controller,
/// which is the one place that knows about HTTP and sets the actual cookies - keeps this layer
/// HTTP-agnostic.</summary>
public record AuthResultDto(UserProfileDto User, Common.Interfaces.IssuedTokens Tokens, bool RequiresTwoFactor);

public record SessionDto(Guid Id, string? DeviceName, string? IpAddress, DateTime LastActiveAt, bool IsCurrent);
