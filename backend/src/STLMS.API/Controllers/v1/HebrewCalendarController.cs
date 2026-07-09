using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Mediator;
using STLMS.Application.JewishCalendar.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hebrew-calendar")]
[RequirePermission("PRAYER_CENTER", "view")]
public class HebrewCalendarController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHebrewDate([FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetHebrewDateQuery(date), ct);
        return Ok(result);
    }
}
