using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.AuditLogs.Queries;

public record ExportAuditLogsQuery : IRequest<ExportFile>;

public class ExportAuditLogsQueryHandler(IUnitOfWork uow, IExportService exportService) : IRequestHandler<ExportAuditLogsQuery, ExportFile>
{
    public Task<ExportFile> HandleAsync(ExportAuditLogsQuery request, CancellationToken ct)
    {
        var logs = uow.Repository<AuditLog>().Query().OrderByDescending(l => l.CreatedAt).Take(5000).ToList();
        var actorIds = logs.Where(l => l.ActorId.HasValue).Select(l => l.ActorId!.Value).ToHashSet();
        var actors = uow.Repository<User>().Query().Where(u => actorIds.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.Email);

        var rows = logs
            .Select(l => (IReadOnlyList<string>)new[]
            {
                l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                l.ActorId.HasValue ? actors.GetValueOrDefault(l.ActorId.Value, "") : "System",
                l.Action,
                l.EntityType,
                l.EntityId?.ToString() ?? "",
                l.Description ?? "",
            })
            .ToList();

        var sheet = new ExportSheet("Audit Log", ["Timestamp", "Actor", "Action", "Entity Type", "Entity Id", "Description"], rows);
        return Task.FromResult(exportService.ToCsv("audit-log", sheet));
    }
}
