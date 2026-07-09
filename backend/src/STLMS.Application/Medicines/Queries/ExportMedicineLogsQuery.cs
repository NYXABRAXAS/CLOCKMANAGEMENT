using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Queries;

public record ExportMedicineLogsQuery(Guid UserId) : IRequest<ExportFile>;

public class ExportMedicineLogsQueryHandler(IUnitOfWork uow, IExportService exportService) : IRequestHandler<ExportMedicineLogsQuery, ExportFile>
{
    public async Task<ExportFile> HandleAsync(ExportMedicineLogsQuery request, CancellationToken ct)
    {
        var medicines = (await uow.Repository<Medicine>().FindAsync(m => m.UserId == request.UserId, ct)).ToDictionary(m => m.Id, m => m.Name);
        var logs = await uow.Repository<MedicineLog>().FindAsync(l => l.UserId == request.UserId, ct);

        var rows = logs
            .OrderByDescending(l => l.ScheduledDate).ThenByDescending(l => l.ScheduledHour).ThenByDescending(l => l.ScheduledMinute)
            .Select(l => (IReadOnlyList<string>)new[]
            {
                l.ScheduledDate.ToString("yyyy-MM-dd"),
                medicines.GetValueOrDefault(l.MedicineId, "(deleted medicine)"),
                $"{l.ScheduledHour:D2}:{l.ScheduledMinute:D2}",
                l.Status.ToString(),
            })
            .ToList();

        var sheet = new ExportSheet("Medicine Logs", ["Date", "Medicine", "Scheduled Time", "Status"], rows);
        return exportService.ToCsv("medicine-logs", sheet);
    }
}
