using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.WorldClock.Commands;

public record ReorderWorldClockCitiesCommand(Guid UserId, IReadOnlyList<Guid> OrderedIds) : IRequest<bool>;

public class ReorderWorldClockCitiesCommandHandler(IUnitOfWork uow) : IRequestHandler<ReorderWorldClockCitiesCommand, bool>
{
    public async Task<bool> HandleAsync(ReorderWorldClockCitiesCommand request, CancellationToken ct)
    {
        var pins = (await uow.Repository<WorldClockCity>().FindAsync(w => w.UserId == request.UserId, ct))
            .ToDictionary(p => p.Id);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            if (!pins.TryGetValue(request.OrderedIds[i], out var pin)) continue;
            pin.SortOrder = i;
            uow.Repository<WorldClockCity>().Update(pin);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
