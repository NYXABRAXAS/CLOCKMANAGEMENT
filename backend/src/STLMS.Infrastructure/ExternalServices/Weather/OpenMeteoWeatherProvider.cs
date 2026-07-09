using System.Globalization;
using System.Text.Json;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.ExternalServices.Weather;

/// <summary>Calls the free, keyless Open-Meteo API (https://open-meteo.com) - registered with
/// HttpClient.BaseAddress = https://api.open-meteo.com/ (see DependencyInjection.AddInfrastructure).
/// Chosen over OpenWeatherMap (which the original config placeholder anticipated) specifically
/// because it needs no signup/API key, which means - like Aladhan and Hebcal before it - this
/// integration can be genuinely live-verified end-to-end instead of shipped as "written but
/// unverified without credentials".</summary>
public class OpenMeteoWeatherProvider(HttpClient httpClient) : IWeatherProvider
{
    public async Task<WeatherResult> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var url = "v1/forecast" +
            $"?latitude={lat}&longitude={lon}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min" +
            "&forecast_days=4&timezone=auto";

        using var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var current = doc.RootElement.GetProperty("current");
        var daily = doc.RootElement.GetProperty("daily");
        var dates = daily.GetProperty("time");
        var codes = daily.GetProperty("weather_code");
        var maxTemps = daily.GetProperty("temperature_2m_max");
        var minTemps = daily.GetProperty("temperature_2m_min");

        var forecast = new List<WeatherDayResult>();
        for (var i = 0; i < dates.GetArrayLength(); i++)
        {
            forecast.Add(new WeatherDayResult(
                DateOnly.Parse(dates[i].GetString()!, CultureInfo.InvariantCulture),
                minTemps[i].GetDouble(),
                maxTemps[i].GetDouble(),
                codes[i].GetInt32()));
        }

        return new WeatherResult(
            current.GetProperty("temperature_2m").GetDouble(),
            current.GetProperty("apparent_temperature").GetDouble(),
            current.GetProperty("relative_humidity_2m").GetInt32(),
            current.GetProperty("wind_speed_10m").GetDouble(),
            current.GetProperty("weather_code").GetInt32(),
            forecast);
    }
}
