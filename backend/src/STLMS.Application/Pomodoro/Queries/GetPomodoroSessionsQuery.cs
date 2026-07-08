using STLMS.Application.Common.Mediator;
using STLMS.Application.Pomodoro.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Pomodoro.Queries;

public record GetPomodoroSessionsQuery(Guid UserId) : IRequest<IReadOnlyList<PomodoroSessionDto>>;

public class GetPomodoroSessionsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetPomodoroSessionsQuery, IReadOnlyList<PomodoroSessionDto>>
{
    public async Task<IReadOnlyList<PomodoroSessionDto>> HandleAsync(GetPomodoroSessionsQuery request, CancellationToken ct)
    {
        var sessions = await uow.Repository<PomodoroSession>().FindAsync(s => s.UserId == request.UserId, ct);
        var sessionIds = sessions.Select(s => s.Id).ToHashSet();
        var logs = (await uow.Repository<PomodoroLog>().FindAsync(l => sessionIds.Contains(l.PomodoroSessionId), ct))
            .GroupBy(l => l.PomodoroSessionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return sessions
            .OrderByDescending(s => s.StartedAt)
            .Take(50)
            .Select(s => PomodoroMapping.ToDto(s, logs.TryGetValue(s.Id, out var sessionLogs) ? sessionLogs : []))
            .ToList();
    }
}
