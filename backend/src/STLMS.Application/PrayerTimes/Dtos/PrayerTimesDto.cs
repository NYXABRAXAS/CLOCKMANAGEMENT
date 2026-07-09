namespace STLMS.Application.PrayerTimes.Dtos;

public record PrayerTimesDto(
    string Fajr,
    string Sunrise,
    string Dhuhr,
    string Asr,
    string Maghrib,
    string Isha,
    string HijriDay,
    string HijriMonth,
    string HijriYear,
    double QiblaDirectionDegrees);

public record PrayerLogDto(DateOnly Date, string PrayerName, bool Completed);
