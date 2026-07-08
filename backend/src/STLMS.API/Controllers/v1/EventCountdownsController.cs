using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.EventCountdowns.Commands;
using STLMS.Application.EventCountdowns.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/event-countdowns")]
public class EventCountdownsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("CALENDAR", "view")]
    [HttpGet]
    public async Task<IActionResult> GetCountdowns(CancellationToken ct)
    {
        var countdowns = await mediator.SendAsync(new GetEventCountdownsQuery(currentUser.UserId!.Value), ct);
        return Ok(countdowns);
    }

    [RequirePermission("CALENDAR", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateCountdown(CreateEventCountdownRequest request, CancellationToken ct)
    {
        var created = await mediator.SendAsync(
            new CreateEventCountdownCommand(currentUser.UserId!.Value, request.Title, request.TargetDate, request.Emoji, request.Color), ct);
        return Ok(created);
    }

    [RequirePermission("CALENDAR", "delete")]
    [HttpDelete("{eventCountdownId:guid}")]
    public async Task<IActionResult> DeleteCountdown(Guid eventCountdownId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteEventCountdownCommand(currentUser.UserId!.Value, eventCountdownId), ct);
        return Ok(new { success = true });
    }
}
