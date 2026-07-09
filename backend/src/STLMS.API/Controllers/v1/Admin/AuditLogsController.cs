using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Admin.AuditLogs.Queries;
using STLMS.Application.Common.Mediator;

namespace STLMS.API.Controllers.v1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/audit-logs")]
[RequirePermission("AUDIT_LOGS", "view")]
public class AuditLogsController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.SendAsync(new GetAuditLogsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var file = await mediator.SendAsync(new ExportAuditLogsQuery(), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
