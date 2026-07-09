using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.JewishCalendar.Dtos;

namespace STLMS.Application.JewishCalendar.Queries;

public record GetHebrewDateQuery(DateOnly Date) : IRequest<HebrewDateDto>;

public class GetHebrewDateQueryHandler(IHebrewCalendarProvider provider, ICacheService cache) : IRequestHandler<GetHebrewDateQuery, HebrewDateDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(7);

    public async Task<HebrewDateDto> HandleAsync(GetHebrewDateQuery request, CancellationToken ct)
    {
        var cacheKey = $"hebrew-date:{request.Date:yyyy-MM-dd}";
        var cached = await cache.GetAsync<HebrewDateDto>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await provider.GetHebrewDateAsync(request.Date, ct);
        var dto = new HebrewDateDto(result.HebrewYear, result.HebrewMonth, result.HebrewDay, result.Formatted, result.Events);

        await cache.SetAsync(cacheKey, dto, CacheTtl, ct);
        return dto;
    }
}
