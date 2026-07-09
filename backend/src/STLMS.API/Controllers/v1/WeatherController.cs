using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Weather.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/weather")]
[RequirePermission("WEATHER", "view")]
public class WeatherController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWeather(CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetWeatherQuery(currentUser.UserId!.Value), ct);
        return Ok(result);
    }
}
