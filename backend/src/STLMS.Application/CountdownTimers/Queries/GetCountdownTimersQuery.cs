using STLMS.Application.Common.Mediator;
using STLMS.Application.CountdownTimers.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CountdownTimers.Queries;

public record GetCountdownTimersQuery(Guid UserId) : IRequest<IReadOnlyList<CountdownTimerDto>>;

public class GetCountdownTimersQueryHandler(IUnitOfWork uow) : IRequestHandler<GetCountdownTimersQuery, IReadOnlyList<CountdownTimerDto>>
{
    public async Task<IReadOnlyList<CountdownTimerDto>> HandleAsync(GetCountdownTimersQuery request, CancellationToken ct)
    {
        var timers = await uow.Repository<CountdownTimer>().FindAsync(t => t.UserId == request.UserId, ct);
        return timers
            .OrderBy(t => t.DurationSeconds)
            .Select(t => new CountdownTimerDto(t.Id, t.Label, t.DurationSeconds, t.SoundId))
            .ToList();
    }
}
