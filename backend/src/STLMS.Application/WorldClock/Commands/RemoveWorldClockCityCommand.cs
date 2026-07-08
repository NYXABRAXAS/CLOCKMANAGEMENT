using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.WorldClock.Commands;

public record RemoveWorldClockCityCommand(Guid UserId, Guid WorldClockCityId) : IRequest<bool>;

public class RemoveWorldClockCityCommandHandler(IUnitOfWork uow) : IRequestHandler<RemoveWorldClockCityCommand, bool>
{
    public async Task<bool> HandleAsync(RemoveWorldClockCityCommand request, CancellationToken ct)
    {
        var pin = await uow.Repository<WorldClockCity>().GetByIdAsync(request.WorldClockCityId, ct);
        if (pin is null || pin.UserId != request.UserId) throw new NotFoundException("WorldClockCity", request.WorldClockCityId);

        uow.Repository<WorldClockCity>().Remove(pin);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
