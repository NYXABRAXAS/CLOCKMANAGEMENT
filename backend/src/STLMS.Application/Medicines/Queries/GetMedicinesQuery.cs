using STLMS.Application.Common.Mediator;
using STLMS.Application.Medicines.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Queries;

public record GetMedicinesQuery(Guid UserId) : IRequest<IReadOnlyList<MedicineDto>>;

public class GetMedicinesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetMedicinesQuery, IReadOnlyList<MedicineDto>>
{
    public async Task<IReadOnlyList<MedicineDto>> HandleAsync(GetMedicinesQuery request, CancellationToken ct)
    {
        var medicines = await uow.Repository<Medicine>().FindAsync(m => m.UserId == request.UserId, ct);
        if (medicines.Count == 0) return [];

        var medicineIds = medicines.Select(m => m.Id).ToHashSet();
        var times = (await uow.Repository<MedicineTime>().FindAsync(t => medicineIds.Contains(t.MedicineId), ct))
            .GroupBy(t => t.MedicineId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return medicines
            .OrderBy(m => m.Name)
            .Select(m => MedicineMapping.ToDto(m, times.TryGetValue(m.Id, out var t) ? t : []))
            .ToList();
    }
}
