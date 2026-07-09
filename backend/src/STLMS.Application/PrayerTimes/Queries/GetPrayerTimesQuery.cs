using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.PrayerTimes.Dtos;
using STLMS.Application.ReligionCalculators;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.PrayerTimes.Queries;

public record GetPrayerTimesQuery(Guid UserId, DateOnly Date) : IRequest<PrayerTimesDto>;

public class GetPrayerTimesQueryHandler(IUnitOfWork uow, IPrayerTimeProvider prayerTimeProvider, ICacheService cache)
    : IRequestHandler<GetPrayerTimesQuery, PrayerTimesDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(2);

    public async Task<PrayerTimesDto> HandleAsync(GetPrayerTimesQuery request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);
        if (user.PrayerLatitude is not { } lat || user.PrayerLongitude is not { } lon)
        {
            throw new ConflictException("Set your prayer location (latitude/longitude) in Settings first.");
        }

        var cacheKey = $"prayer-times:{lat:F4}:{lon:F4}:{user.PrayerCalculationMethod ?? 2}:{request.Date:yyyy-MM-dd}";
        var cached = await cache.GetAsync<PrayerTimesDto>(cacheKey, ct);
        if (cached is not null) return cached with { QiblaDirectionDegrees = QiblaCalculator.BearingDegrees(lat, lon) };

        var result = await prayerTimeProvider.GetPrayerTimesAsync(lat, lon, request.Date, user.PrayerCalculationMethod, ct);
        var qibla = QiblaCalculator.BearingDegrees(lat, lon);

        var dto = new PrayerTimesDto(
            result.Fajr, result.Sunrise, result.Dhuhr, result.Asr, result.Maghrib, result.Isha,
            result.HijriDay, result.HijriMonth, result.HijriYear, qibla);

        await cache.SetAsync(cacheKey, dto, CacheTtl, ct);
        return dto;
    }
}
