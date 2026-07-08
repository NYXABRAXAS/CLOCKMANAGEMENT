using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Cities.Queries;

public record CityDto(Guid Id, string Name, string Country, string CountryCode, string TimezoneId, double Latitude, double Longitude);

public record SearchCitiesQuery(string? Search) : IRequest<IReadOnlyList<CityDto>>;

public class SearchCitiesQueryHandler(IUnitOfWork uow) : IRequestHandler<SearchCitiesQuery, IReadOnlyList<CityDto>>
{
    public async Task<IReadOnlyList<CityDto>> HandleAsync(SearchCitiesQuery request, CancellationToken ct)
    {
        // .ToLower() (rather than relying on provider collation) keeps matching consistent across
        // Sqlite/SqlServer/Postgres - Postgres's default collation is case-sensitive, unlike the
        // other two.
        var cities = string.IsNullOrWhiteSpace(request.Search)
            ? await uow.Repository<City>().GetAllAsync(ct)
            : await uow.Repository<City>().FindAsync(
                c => c.Name.ToLower().Contains(request.Search.ToLower()) || c.Country.ToLower().Contains(request.Search.ToLower()), ct);

        return cities
            .OrderBy(c => c.Name)
            .Take(50)
            .Select(c => new CityDto(c.Id, c.Name, c.Country, c.CountryCode, c.TimezoneId, c.Latitude, c.Longitude))
            .ToList();
    }
}
