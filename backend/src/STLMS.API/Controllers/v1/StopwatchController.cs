using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.StopwatchSessions.Commands;
using STLMS.Application.StopwatchSessions.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/stopwatch/sessions")]
public class StopwatchController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("TIMERS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        var sessions = await mediator.SendAsync(new GetStopwatchSessionsQuery(currentUser.UserId!.Value), ct);
        return Ok(sessions);
    }

    [RequirePermission("TIMERS", "create")]
    [HttpPost]
    public async Task<IActionResult> SaveSession(SaveStopwatchSessionRequest request, CancellationToken ct)
    {
        var session = await mediator.SendAsync(
            new SaveStopwatchSessionCommand(
                currentUser.UserId!.Value,
                request.Label,
                request.StartedAt,
                request.EndedAt,
                request.TotalDurationMs,
                request.Laps.Select(l => new LapInput(l.LapNumber, l.LapDurationMs, l.CumulativeDurationMs)).ToList()),
            ct);
        return Ok(session);
    }
}
