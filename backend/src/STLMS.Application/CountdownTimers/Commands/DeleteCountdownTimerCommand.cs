using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CountdownTimers.Commands;

public record DeleteCountdownTimerCommand(Guid UserId, Guid CountdownTimerId) : IRequest<bool>;

public class DeleteCountdownTimerCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteCountdownTimerCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCountdownTimerCommand request, CancellationToken ct)
    {
        var timer = await uow.Repository<CountdownTimer>().GetByIdAsync(request.CountdownTimerId, ct);
        if (timer is null || timer.UserId != request.UserId) throw new NotFoundException("CountdownTimer", request.CountdownTimerId);

        uow.Repository<CountdownTimer>().Remove(timer);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
