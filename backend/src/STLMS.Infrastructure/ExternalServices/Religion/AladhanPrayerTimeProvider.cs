using System.Globalization;
using System.Text.Json;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.ExternalServices.Religion;

/// <summary>Calls the free, keyless Aladhan API (https://aladhan.com/prayer-times-api) - registered
/// with HttpClient.BaseAddress = https://api.aladhan.com/ (see DependencyInjection.AddInfrastructure).</summary>
public class AladhanPrayerTimeProvider(HttpClient httpClient) : IPrayerTimeProvider
{
    public async Task<PrayerTimesResult> GetPrayerTimesAsync(double latitude, double longitude, DateOnly date, int? method, CancellationToken ct = default)
    {
        var dateStr = date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var methodId = method ?? 2; // 2 = ISNA, a reasonable default

        using var response = await httpClient.GetAsync($"v1/timings/{dateStr}?latitude={lat}&longitude={lon}&method={methodId}", ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var data = doc.RootElement.GetProperty("data");
        var timings = data.GetProperty("timings");
        var hijri = data.GetProperty("date").GetProperty("hijri");

        // Aladhan sometimes appends a "(TZ)" suffix to times - keep just the HH:mm part.
        string Time(string key) => timings.GetProperty(key).GetString()!.Split(' ')[0];

        return new PrayerTimesResult(
            Time("Fajr"), Time("Sunrise"), Time("Dhuhr"), Time("Asr"), Time("Maghrib"), Time("Isha"),
            hijri.GetProperty("day").GetString()!,
            hijri.GetProperty("month").GetProperty("en").GetString()!,
            hijri.GetProperty("year").GetString()!);
    }
}
