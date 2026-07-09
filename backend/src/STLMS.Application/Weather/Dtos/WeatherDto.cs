namespace STLMS.Application.Weather.Dtos;

public record WeatherForecastDayDto(DateOnly Date, double MinTempC, double MaxTempC, string Condition, string Icon);

public record WeatherDto(
    double TempC,
    double FeelsLikeC,
    int Humidity,
    double WindSpeedKph,
    string Condition,
    string Icon,
    IReadOnlyList<WeatherForecastDayDto> Forecast);
