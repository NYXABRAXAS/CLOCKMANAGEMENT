namespace STLMS.API.Controllers.v1.Requests;

public record UpdateSettingsRequest(
    string? CountryCode,
    string TimezoneId,
    string TimeFormat,
    string Language,
    string Theme,
    string? ReligionCode,
    double? PrayerLatitude,
    double? PrayerLongitude);
