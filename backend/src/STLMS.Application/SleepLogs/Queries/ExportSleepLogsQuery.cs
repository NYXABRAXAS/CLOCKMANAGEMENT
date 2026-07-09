using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.SleepLogs.Queries;

public record ExportSleepLogsQuery(Guid UserId) : IRequest<ExportFile>;

public class ExportSleepLogsQueryHandler(IUnitOfWork uow, IExportService exportService) : IRequestHandler<ExportSleepLogsQuery, ExportFile>
{
    public async Task<ExportFile> HandleAsync(ExportSleepLogsQuery request, CancellationToken ct)
    {
        var logs = await uow.Repository<SleepLog>().FindAsync(l => l.UserId == request.UserId, ct);

        var rows = logs
            .OrderByDescending(l => l.Date)
            .Select(l => (IReadOnlyList<string>)new[]
            {
                l.Date.ToString("yyyy-MM-dd"),
                l.BedTime.ToString("yyyy-MM-dd HH:mm"),
                l.WakeTime.ToString("yyyy-MM-dd HH:mm"),
                (l.DurationMinutes / 60.0).ToString("F1"),
                l.Quality?.ToString() ?? "",
                l.Notes ?? "",
            })
            .ToList();

        var sheet = new ExportSheet("Sleep Logs", ["Date", "Bed Time", "Wake Time", "Duration (hrs)", "Quality", "Notes"], rows);
        return exportService.ToCsv("sleep-logs", sheet);
    }
}
