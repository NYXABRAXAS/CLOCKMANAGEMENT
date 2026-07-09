using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Productivity.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/productivity")]
[RequirePermission("PRODUCTIVITY", "view")]
public class ProductivityController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetProductivitySummaryQuery(currentUser.UserId!.Value, from, to), ct);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string format = "csv", CancellationToken ct = default)
    {
        var file = await mediator.SendAsync(new ExportProductivityReportQuery(currentUser.UserId!.Value, from, to, format), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
