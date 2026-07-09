namespace STLMS.Application.Common.Interfaces;

public record WeatherDayResult(DateOnly Date, double MinTempC, double MaxTempC, int ConditionCode);

public record WeatherResult(
    double TempC,
    double FeelsLikeC,
    int Humidity,
    double WindSpeedKph,
    int ConditionCode,
    IReadOnlyList<WeatherDayResult> Forecast);

public interface IWeatherProvider
{
    Task<WeatherResult> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken ct = default);
}
