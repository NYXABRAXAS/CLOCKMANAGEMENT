using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Commands;

public record LogMedicineDoseCommand(
    Guid UserId, Guid MedicineId, DateOnly ScheduledDate, int ScheduledHour, int ScheduledMinute, MedicineLogStatus Status) : IRequest<bool>;

public class LogMedicineDoseCommandHandler(IUnitOfWork uow) : IRequestHandler<LogMedicineDoseCommand, bool>
{
    public async Task<bool> HandleAsync(LogMedicineDoseCommand request, CancellationToken ct)
    {
        var medicine = await uow.Repository<Medicine>().GetByIdAsync(request.MedicineId, ct);
        if (medicine is null || medicine.UserId != request.UserId) throw new NotFoundException("Medicine", request.MedicineId);

        var existing = await uow.Repository<MedicineLog>().SingleOrDefaultAsync(
            l => l.MedicineId == request.MedicineId && l.ScheduledDate == request.ScheduledDate
                 && l.ScheduledHour == request.ScheduledHour && l.ScheduledMinute == request.ScheduledMinute, ct);

        if (existing is null)
        {
            await uow.Repository<MedicineLog>().AddAsync(
                new MedicineLog
                {
                    MedicineId = request.MedicineId,
                    UserId = request.UserId,
                    ScheduledDate = request.ScheduledDate,
                    ScheduledHour = request.ScheduledHour,
                    ScheduledMinute = request.ScheduledMinute,
                    Status = request.Status,
                    RecordedAt = DateTime.UtcNow,
                },
                ct);
        }
        else
        {
            existing.Status = request.Status;
            existing.RecordedAt = DateTime.UtcNow;
            uow.Repository<MedicineLog>().Update(existing);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
