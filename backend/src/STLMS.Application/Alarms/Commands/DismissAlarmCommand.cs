using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

public record DismissAlarmCommand(Guid UserId, Guid AlarmId) : IRequest<bool>;

public class DismissAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<DismissAlarmCommand, bool>
{
    public async Task<bool> HandleAsync(DismissAlarmCommand request, CancellationToken ct)
    {
        var alarm = await uow.Repository<Alarm>().GetByIdAsync(request.AlarmId, ct);
        if (alarm is null || alarm.UserId != request.UserId) throw new NotFoundException("Alarm", request.AlarmId);

        await uow.Repository<AlarmHistory>().AddAsync(
            new AlarmHistory { AlarmId = alarm.Id, UserId = request.UserId, Action = AlarmHistoryAction.Dismissed }, ct);

        // One-time alarms (no repeat days) auto-disable after firing once, same as any physical
        // alarm clock's "one-off" mode.
        if (alarm.RepeatDaysMask == 0)
        {
            alarm.IsEnabled = false;
            uow.Repository<Alarm>().Update(alarm);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
