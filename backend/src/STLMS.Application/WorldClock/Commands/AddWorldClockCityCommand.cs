using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.WorldClock.Commands;

public record AddWorldClockCityCommand(Guid UserId, Guid CityId) : IRequest<Guid>;

public class AddWorldClockCityCommandValidator : AbstractValidator<AddWorldClockCityCommand>
{
    public AddWorldClockCityCommandValidator()
    {
        RuleFor(x => x.CityId).NotEmpty();
    }
}

public class AddWorldClockCityCommandHandler(IUnitOfWork uow) : IRequestHandler<AddWorldClockCityCommand, Guid>
{
    public async Task<Guid> HandleAsync(AddWorldClockCityCommand request, CancellationToken ct)
    {
        var city = await uow.Repository<City>().GetByIdAsync(request.CityId, ct)
            ?? throw new NotFoundException("City", request.CityId);

        var existing = await uow.Repository<WorldClockCity>()
            .SingleOrDefaultAsync(w => w.UserId == request.UserId && w.CityId == request.CityId, ct);
        if (existing is not null) throw new ConflictException($"{city.Name} is already pinned to your World Clock.");

        var pins = await uow.Repository<WorldClockCity>().FindAsync(w => w.UserId == request.UserId, ct);
        var nextSortOrder = pins.Count == 0 ? 0 : pins.Max(p => p.SortOrder) + 1;

        var pin = new WorldClockCity { UserId = request.UserId, CityId = request.CityId, SortOrder = nextSortOrder };
        await uow.Repository<WorldClockCity>().AddAsync(pin, ct);
        await uow.SaveChangesAsync(ct);

        return pin.Id;
    }
}
