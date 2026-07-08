using STLMS.Application.Cities.Queries;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.WorldClock.Queries;

public record WorldClockCityDto(Guid Id, CityDto City, int SortOrder);

public record GetWorldClockCitiesQuery(Guid UserId) : IRequest<IReadOnlyList<WorldClockCityDto>>;

public class GetWorldClockCitiesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetWorldClockCitiesQuery, IReadOnlyList<WorldClockCityDto>>
{
    public async Task<IReadOnlyList<WorldClockCityDto>> HandleAsync(GetWorldClockCitiesQuery request, CancellationToken ct)
    {
        var pins = await uow.Repository<WorldClockCity>().FindAsync(w => w.UserId == request.UserId, ct);
        if (pins.Count == 0) return [];

        var cityIds = pins.Select(p => p.CityId).ToHashSet();
        var cities = (await uow.Repository<City>().FindAsync(c => cityIds.Contains(c.Id), ct)).ToDictionary(c => c.Id);

        return pins
            .OrderBy(p => p.SortOrder)
            .Where(p => cities.ContainsKey(p.CityId))
            .Select(p =>
            {
                var c = cities[p.CityId];
                return new WorldClockCityDto(p.Id, new CityDto(c.Id, c.Name, c.Country, c.CountryCode, c.TimezoneId, c.Latitude, c.Longitude), p.SortOrder);
            })
            .ToList();
    }
}
