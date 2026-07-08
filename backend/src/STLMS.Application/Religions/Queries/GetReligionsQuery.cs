using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Religions.Queries;

public record ReligionDto(Guid Id, string Code, string Name, int SortOrder);

public record GetReligionsQuery : IRequest<IReadOnlyList<ReligionDto>>;

public class GetReligionsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetReligionsQuery, IReadOnlyList<ReligionDto>>
{
    public async Task<IReadOnlyList<ReligionDto>> HandleAsync(GetReligionsQuery request, CancellationToken ct)
    {
        var religions = await uow.Repository<Religion>().GetAllAsync(ct);
        return religions
            .OrderBy(r => r.SortOrder)
            .Select(r => new ReligionDto(r.Id, r.Code, r.Name, r.SortOrder))
            .ToList();
    }
}
