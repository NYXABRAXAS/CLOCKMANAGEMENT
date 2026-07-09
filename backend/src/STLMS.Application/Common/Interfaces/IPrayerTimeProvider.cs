namespace STLMS.Application.Common.Interfaces;

public record PrayerTimesResult(
    string Fajr,
    string Sunrise,
    string Dhuhr,
    string Asr,
    string Maghrib,
    string Isha,
    string HijriDay,
    string HijriMonth,
    string HijriYear);

/// <summary>Real prayer-time calculation is a genuine astronomical + jurisprudential problem (twilight
/// angle conventions differ by calculation method/school) - deliberately not hand-rolled. Backed by
/// the free, keyless Aladhan API.</summary>
public interface IPrayerTimeProvider
{
    Task<PrayerTimesResult> GetPrayerTimesAsync(double latitude, double longitude, DateOnly date, int? method, CancellationToken ct = default);
}
