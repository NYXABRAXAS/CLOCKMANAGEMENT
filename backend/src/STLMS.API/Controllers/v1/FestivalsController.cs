using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Festivals.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/festivals")]
[RequirePermission("PRAYER_CENTER", "view")]
public class FestivalsController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUpcoming([FromQuery] Guid? religionId, [FromQuery] int daysAhead, CancellationToken ct)
    {
        var festivals = await mediator.SendAsync(new GetUpcomingFestivalsQuery(religionId, daysAhead == 0 ? 90 : daysAhead), ct);
        return Ok(festivals);
    }
}
