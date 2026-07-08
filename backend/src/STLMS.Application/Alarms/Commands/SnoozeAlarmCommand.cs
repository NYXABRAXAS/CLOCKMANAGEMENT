using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

/// <summary>Just logs the snooze event - the actual "ring again in N minutes" re-scheduling
/// happens client-side (the client already knows SnoozeMinutes from the alarm's own settings).</summary>
public record SnoozeAlarmCommand(Guid UserId, Guid AlarmId) : IRequest<bool>;

public class SnoozeAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<SnoozeAlarmCommand, bool>
{
    public async Task<bool> HandleAsync(SnoozeAlarmCommand request, CancellationToken ct)
    {
        var alarm = await uow.Repository<Alarm>().GetByIdAsync(request.AlarmId, ct);
        if (alarm is null || alarm.UserId != request.UserId) throw new NotFoundException("Alarm", request.AlarmId);

        await uow.Repository<AlarmHistory>().AddAsync(
            new AlarmHistory { AlarmId = alarm.Id, UserId = request.UserId, Action = AlarmHistoryAction.Snoozed }, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
