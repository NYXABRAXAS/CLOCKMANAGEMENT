using System.Text.Json;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.ExternalServices.Religion;

/// <summary>Calls the free, keyless Hebcal API (https://www.hebcal.com/home/developer-apis) -
/// registered with HttpClient.BaseAddress = https://www.hebcal.com/ (see
/// DependencyInjection.AddInfrastructure).</summary>
public class HebcalProvider(HttpClient httpClient) : IHebrewCalendarProvider
{
    public async Task<HebrewDateResult> GetHebrewDateAsync(DateOnly date, CancellationToken ct = default)
    {
        using var response = await httpClient.GetAsync($"converter?cfg=json&gy={date.Year}&gm={date.Month}&gd={date.Day}&g2h=1", ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;

        var events = new List<string>();
        if (root.TryGetProperty("events", out var eventsEl) && eventsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in eventsEl.EnumerateArray())
            {
                var value = e.GetString();
                if (value is not null) events.Add(value);
            }
        }

        return new HebrewDateResult(
            root.GetProperty("hy").GetInt32(),
            root.GetProperty("hm").GetString()!,
            root.GetProperty("hd").GetInt32(),
            root.GetProperty("hebrew").GetString()!,
            events);
    }
}
