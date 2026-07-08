using STLMS.Application.Alarms.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

public record ToggleAlarmCommand(Guid UserId, Guid AlarmId, bool IsEnabled) : IRequest<AlarmDto>;

public class ToggleAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<ToggleAlarmCommand, AlarmDto>
{
    public async Task<AlarmDto> HandleAsync(ToggleAlarmCommand request, CancellationToken ct)
    {
        var alarm = await uow.Repository<Alarm>().GetByIdAsync(request.AlarmId, ct);
        if (alarm is null || alarm.UserId != request.UserId) throw new NotFoundException("Alarm", request.AlarmId);

        alarm.IsEnabled = request.IsEnabled;
        uow.Repository<Alarm>().Update(alarm);
        await uow.SaveChangesAsync(ct);
        return AlarmMapping.ToDto(alarm);
    }
}
