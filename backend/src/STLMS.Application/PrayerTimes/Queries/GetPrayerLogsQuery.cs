using STLMS.Application.Common.Mediator;
using STLMS.Application.PrayerTimes.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.PrayerTimes.Queries;

public record GetPrayerLogsQuery(Guid UserId, DateOnly Date) : IRequest<IReadOnlyList<PrayerLogDto>>;

public class GetPrayerLogsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetPrayerLogsQuery, IReadOnlyList<PrayerLogDto>>
{
    public async Task<IReadOnlyList<PrayerLogDto>> HandleAsync(GetPrayerLogsQuery request, CancellationToken ct)
    {
        var logs = await uow.Repository<UserPrayerLog>().FindAsync(l => l.UserId == request.UserId && l.Date == request.Date, ct);
        return logs.Select(l => new PrayerLogDto(l.Date, l.PrayerName, l.Completed)).ToList();
    }
}
