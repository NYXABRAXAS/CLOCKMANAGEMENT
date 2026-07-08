using STLMS.Application.Alarms.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Queries;

public record GetAlarmsQuery(Guid UserId) : IRequest<IReadOnlyList<AlarmDto>>;

public class GetAlarmsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAlarmsQuery, IReadOnlyList<AlarmDto>>
{
    public async Task<IReadOnlyList<AlarmDto>> HandleAsync(GetAlarmsQuery request, CancellationToken ct)
    {
        var alarms = await uow.Repository<Alarm>().FindAsync(a => a.UserId == request.UserId, ct);
        return alarms
            .OrderBy(a => a.Hour).ThenBy(a => a.Minute)
            .Select(AlarmMapping.ToDto)
            .ToList();
    }
}
