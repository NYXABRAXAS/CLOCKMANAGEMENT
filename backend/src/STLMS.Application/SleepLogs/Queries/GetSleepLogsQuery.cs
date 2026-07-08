using STLMS.Application.Common.Mediator;
using STLMS.Application.SleepLogs.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.SleepLogs.Queries;

public record GetSleepLogsQuery(Guid UserId) : IRequest<IReadOnlyList<SleepLogDto>>;

public class GetSleepLogsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetSleepLogsQuery, IReadOnlyList<SleepLogDto>>
{
    public async Task<IReadOnlyList<SleepLogDto>> HandleAsync(GetSleepLogsQuery request, CancellationToken ct)
    {
        var logs = await uow.Repository<SleepLog>().FindAsync(l => l.UserId == request.UserId, ct);
        return logs
            .OrderByDescending(l => l.Date)
            .Take(60)
            .Select(l => new SleepLogDto(l.Id, l.Date, l.BedTime, l.WakeTime, l.DurationMinutes, l.Quality?.ToString(), l.Notes))
            .ToList();
    }
}
