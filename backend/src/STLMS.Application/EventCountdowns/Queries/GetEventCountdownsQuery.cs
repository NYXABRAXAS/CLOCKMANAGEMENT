using STLMS.Application.Common.Mediator;
using STLMS.Application.EventCountdowns.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.EventCountdowns.Queries;

public record GetEventCountdownsQuery(Guid UserId) : IRequest<IReadOnlyList<EventCountdownDto>>;

public class GetEventCountdownsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetEventCountdownsQuery, IReadOnlyList<EventCountdownDto>>
{
    public async Task<IReadOnlyList<EventCountdownDto>> HandleAsync(GetEventCountdownsQuery request, CancellationToken ct)
    {
        var countdowns = await uow.Repository<EventCountdown>().FindAsync(c => c.UserId == request.UserId, ct);
        return countdowns
            .OrderBy(c => c.TargetDate)
            .Select(c => new EventCountdownDto(c.Id, c.Title, c.TargetDate, c.Emoji, c.Color))
            .ToList();
    }
}
