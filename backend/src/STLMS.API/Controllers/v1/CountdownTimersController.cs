using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.CountdownTimers.Commands;
using STLMS.Application.CountdownTimers.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/countdown-timers")]
public class CountdownTimersController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("TIMERS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetTimers(CancellationToken ct)
    {
        var timers = await mediator.SendAsync(new GetCountdownTimersQuery(currentUser.UserId!.Value), ct);
        return Ok(timers);
    }

    [RequirePermission("TIMERS", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateTimer(CreateCountdownTimerRequest request, CancellationToken ct)
    {
        var timer = await mediator.SendAsync(
            new CreateCountdownTimerCommand(currentUser.UserId!.Value, request.Label, request.DurationSeconds, request.SoundId), ct);
        return Ok(timer);
    }

    [RequirePermission("TIMERS", "delete")]
    [HttpDelete("{countdownTimerId:guid}")]
    public async Task<IActionResult> DeleteTimer(Guid countdownTimerId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteCountdownTimerCommand(currentUser.UserId!.Value, countdownTimerId), ct);
        return Ok(new { success = true });
    }
}
