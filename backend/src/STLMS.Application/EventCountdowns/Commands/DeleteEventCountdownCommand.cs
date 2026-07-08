using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.EventCountdowns.Commands;

public record DeleteEventCountdownCommand(Guid UserId, Guid EventCountdownId) : IRequest<bool>;

public class DeleteEventCountdownCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteEventCountdownCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteEventCountdownCommand request, CancellationToken ct)
    {
        var countdown = await uow.Repository<EventCountdown>().GetByIdAsync(request.EventCountdownId, ct);
        if (countdown is null || countdown.UserId != request.UserId) throw new NotFoundException("EventCountdown", request.EventCountdownId);

        uow.Repository<EventCountdown>().Remove(countdown);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
