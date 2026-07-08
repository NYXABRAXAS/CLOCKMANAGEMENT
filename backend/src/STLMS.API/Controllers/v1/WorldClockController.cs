using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.WorldClock.Commands;
using STLMS.Application.WorldClock.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/world-clock")]
public class WorldClockController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("WORLD_CLOCK", "view")]
    [HttpGet]
    public async Task<IActionResult> GetPinnedCities(CancellationToken ct)
    {
        var cities = await mediator.SendAsync(new GetWorldClockCitiesQuery(currentUser.UserId!.Value), ct);
        return Ok(cities);
    }

    [RequirePermission("WORLD_CLOCK", "create")]
    [HttpPost]
    public async Task<IActionResult> AddCity(AddWorldClockCityRequest request, CancellationToken ct)
    {
        var id = await mediator.SendAsync(new AddWorldClockCityCommand(currentUser.UserId!.Value, request.CityId), ct);
        return Ok(new { id });
    }

    [RequirePermission("WORLD_CLOCK", "edit")]
    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(ReorderWorldClockCitiesRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new ReorderWorldClockCitiesCommand(currentUser.UserId!.Value, request.OrderedIds), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("WORLD_CLOCK", "delete")]
    [HttpDelete("{worldClockCityId:guid}")]
    public async Task<IActionResult> RemoveCity(Guid worldClockCityId, CancellationToken ct)
    {
        await mediator.SendAsync(new RemoveWorldClockCityCommand(currentUser.UserId!.Value, worldClockCityId), ct);
        return Ok(new { success = true });
    }
}
