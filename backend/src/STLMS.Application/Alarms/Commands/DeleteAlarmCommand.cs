using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

public record DeleteAlarmCommand(Guid UserId, Guid AlarmId) : IRequest<bool>;

public class DeleteAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteAlarmCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteAlarmCommand request, CancellationToken ct)
    {
        var alarm = await uow.Repository<Alarm>().GetByIdAsync(request.AlarmId, ct);
        if (alarm is null || alarm.UserId != request.UserId) throw new NotFoundException("Alarm", request.AlarmId);

        uow.Repository<Alarm>().Remove(alarm);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
