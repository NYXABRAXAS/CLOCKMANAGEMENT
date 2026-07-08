using STLMS.Application.Common.Mediator;
using STLMS.Application.StopwatchSessions.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.StopwatchSessions.Queries;

public record GetStopwatchSessionsQuery(Guid UserId) : IRequest<IReadOnlyList<StopwatchSessionDto>>;

public class GetStopwatchSessionsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetStopwatchSessionsQuery, IReadOnlyList<StopwatchSessionDto>>
{
    public async Task<IReadOnlyList<StopwatchSessionDto>> HandleAsync(GetStopwatchSessionsQuery request, CancellationToken ct)
    {
        var sessions = await uow.Repository<STLMS.Domain.Entities.StopwatchSession>().FindAsync(s => s.UserId == request.UserId, ct);
        var sessionIds = sessions.Select(s => s.Id).ToHashSet();
        var laps = (await uow.Repository<StopwatchLap>().FindAsync(l => sessionIds.Contains(l.StopwatchSessionId), ct))
            .GroupBy(l => l.StopwatchSessionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.LapNumber).ToList());

        return sessions
            .OrderByDescending(s => s.StartedAt)
            .Take(50)
            .Select(s => new StopwatchSessionDto(
                s.Id, s.Label, s.StartedAt, s.EndedAt, s.TotalDurationMs,
                (laps.TryGetValue(s.Id, out var sessionLaps) ? sessionLaps : [])
                    .Select(l => new StopwatchLapDto(l.LapNumber, l.LapDurationMs, l.CumulativeDurationMs))
                    .ToList()))
            .ToList();
    }
}
