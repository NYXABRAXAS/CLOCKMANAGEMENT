using STLMS.Application.Auth.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.Auth;

/// <summary>Hand-written mapping for the one Auth-facing shape (User -> UserProfileDto) instead
/// of a generic Mapperly mapper - the roles/permissions projection needs the RolePermissions
/// already loaded and flattened, which reads more clearly as a plain method than a generated
/// mapper attribute here.</summary>
public static class AuthMapping
{
    public static UserProfileDto ToProfileDto(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions) =>
        new(
            user.Id,
            user.Email,
            user.EmailVerified,
            user.FirstName,
            user.LastName,
            user.PhotoUrl,
            user.Religion?.Code,
            user.CountryCode,
            user.TimezoneId,
            user.TimeFormat,
            user.Language,
            user.Theme,
            user.PrayerLatitude,
            user.PrayerLongitude,
            user.PrayerCalculationMethod,
            user.WeatherLatitude,
            user.WeatherLongitude,
            user.EmailNotificationsEnabled,
            user.PushNotificationsEnabled,
            user.SubscriptionStatus.ToString(),
            user.TwoFactorEnabled,
            roles,
            permissions);
}
