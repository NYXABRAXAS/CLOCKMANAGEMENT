using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Queries;

public record ExportHabitLogsQuery(Guid UserId) : IRequest<ExportFile>;

public class ExportHabitLogsQueryHandler(IUnitOfWork uow, IExportService exportService) : IRequestHandler<ExportHabitLogsQuery, ExportFile>
{
    public async Task<ExportFile> HandleAsync(ExportHabitLogsQuery request, CancellationToken ct)
    {
        var habits = (await uow.Repository<Habit>().FindAsync(h => h.UserId == request.UserId, ct)).ToDictionary(h => h.Id, h => h.Title);
        var logs = await uow.Repository<HabitLog>().FindAsync(l => l.UserId == request.UserId, ct);

        var rows = logs
            .OrderByDescending(l => l.Date)
            .Select(l => (IReadOnlyList<string>)new[] { l.Date.ToString("yyyy-MM-dd"), habits.GetValueOrDefault(l.HabitId, "(deleted habit)"), l.Completed ? "Yes" : "No" })
            .ToList();

        var sheet = new ExportSheet("Habit Logs", ["Date", "Habit", "Completed"], rows);
        return exportService.ToCsv("habit-logs", sheet);
    }
}
