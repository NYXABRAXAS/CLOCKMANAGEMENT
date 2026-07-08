using STLMS.Application.Common.Mediator;
using STLMS.Application.Medicines.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Queries;

public record GetMedicineLogsQuery(Guid UserId, DateOnly Date) : IRequest<IReadOnlyList<MedicineLogDto>>;

public class GetMedicineLogsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetMedicineLogsQuery, IReadOnlyList<MedicineLogDto>>
{
    public async Task<IReadOnlyList<MedicineLogDto>> HandleAsync(GetMedicineLogsQuery request, CancellationToken ct)
    {
        var logs = await uow.Repository<MedicineLog>().FindAsync(l => l.UserId == request.UserId && l.ScheduledDate == request.Date, ct);
        return logs
            .Select(l => new MedicineLogDto(l.MedicineId, l.ScheduledDate, l.ScheduledHour, l.ScheduledMinute, l.Status.ToString()))
            .ToList();
    }
}
