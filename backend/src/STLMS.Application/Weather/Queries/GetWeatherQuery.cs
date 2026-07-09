using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Weather.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Weather.Queries;

public record GetWeatherQuery(Guid UserId) : IRequest<WeatherDto>;

public class GetWeatherQueryHandler(IUnitOfWork uow, IWeatherProvider weatherProvider, ICacheService cache)
    : IRequestHandler<GetWeatherQuery, WeatherDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public async Task<WeatherDto> HandleAsync(GetWeatherQuery request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);
        if (user.WeatherLatitude is not { } lat || user.WeatherLongitude is not { } lon)
        {
            throw new ConflictException("Set your weather location (latitude/longitude) in Settings first.");
        }

        var cacheKey = $"weather:{lat:F2}:{lon:F2}";
        var cached = await cache.GetAsync<WeatherDto>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await weatherProvider.GetCurrentWeatherAsync(lat, lon, ct);
        var (condition, icon) = WmoWeatherCodes.Describe(result.ConditionCode);

        var dto = new WeatherDto(
            result.TempC,
            result.FeelsLikeC,
            result.Humidity,
            result.WindSpeedKph,
            condition,
            icon,
            result.Forecast.Select(f =>
            {
                var (dayCondition, dayIcon) = WmoWeatherCodes.Describe(f.ConditionCode);
                return new WeatherForecastDayDto(f.Date, f.MinTempC, f.MaxTempC, dayCondition, dayIcon);
            }).ToList());

        await cache.SetAsync(cacheKey, dto, CacheTtl, ct);
        return dto;
    }
}
