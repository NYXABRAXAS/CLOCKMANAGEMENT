using STLMS.Application.Admin.AuditLogs.Dtos;
using STLMS.Application.Common.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.AuditLogs.Queries;

public record GetAuditLogsQuery(int Page, int PageSize) : IRequest<PagedResult<AuditLogDto>>;

public class GetAuditLogsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public Task<PagedResult<AuditLogDto>> HandleAsync(GetAuditLogsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = uow.Repository<AuditLog>().Query();
        var totalCount = query.Count();
        var logs = query.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var actorIds = logs.Where(l => l.ActorId.HasValue).Select(l => l.ActorId!.Value).ToHashSet();
        var actors = uow.Repository<User>().Query().Where(u => actorIds.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.Email);

        var items = logs
            .Select(l => new AuditLogDto(
                l.Id, l.ActorId, l.ActorId.HasValue ? actors.GetValueOrDefault(l.ActorId.Value) : null, l.Action, l.EntityType, l.EntityId,
                l.Description, l.IpAddress, l.CreatedAt))
            .ToList();

        return Task.FromResult(new PagedResult<AuditLogDto>(items, totalCount, page, pageSize));
    }
}
