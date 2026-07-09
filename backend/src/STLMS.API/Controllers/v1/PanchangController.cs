using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Panchang.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/panchang")]
[RequirePermission("PRAYER_CENTER", "view")]
public class PanchangController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPanchang([FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetPanchangQuery(date), ct);
        return Ok(result);
    }
}
