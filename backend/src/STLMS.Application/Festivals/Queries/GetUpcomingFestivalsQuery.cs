using STLMS.Application.Common.Mediator;
using STLMS.Application.Festivals.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Festivals.Queries;

/// <summary>ReligionId null = festivals across all religions (a "what's coming up everywhere"
/// view); set = just that religion's calendar.</summary>
public record GetUpcomingFestivalsQuery(Guid? ReligionId, int DaysAhead) : IRequest<IReadOnlyList<FestivalDto>>;

public class GetUpcomingFestivalsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetUpcomingFestivalsQuery, IReadOnlyList<FestivalDto>>
{
    public async Task<IReadOnlyList<FestivalDto>> HandleAsync(GetUpcomingFestivalsQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeEnd = today.AddDays(Math.Max(1, request.DaysAhead));

        var entries = request.ReligionId is { } religionId
            ? await uow.Repository<FestivalCalendarEntry>().FindAsync(f => f.ReligionId == religionId && f.Date >= today && f.Date <= rangeEnd, ct)
            : await uow.Repository<FestivalCalendarEntry>().FindAsync(f => f.Date >= today && f.Date <= rangeEnd, ct);
        if (entries.Count == 0) return [];

        var religionIds = entries.Select(e => e.ReligionId).ToHashSet();
        var religions = (await uow.Repository<Religion>().FindAsync(r => religionIds.Contains(r.Id), ct)).ToDictionary(r => r.Id);

        return entries
            .Where(e => religions.ContainsKey(e.ReligionId))
            .OrderBy(e => e.Date)
            .Select(e =>
            {
                var religion = religions[e.ReligionId];
                return new FestivalDto(e.Id, religion.Code, religion.Name, e.Name, e.Description, e.Date, e.Emoji, e.Date.DayNumber - today.DayNumber);
            })
            .ToList();
    }
}
