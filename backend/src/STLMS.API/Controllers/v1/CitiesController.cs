using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Cities.Queries;
using STLMS.Application.Common.Mediator;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cities")]
[RequirePermission("WORLD_CLOCK", "view")]
public class CitiesController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, CancellationToken ct)
    {
        var cities = await mediator.SendAsync(new SearchCitiesQuery(search), ct);
        return Ok(cities);
    }
}
