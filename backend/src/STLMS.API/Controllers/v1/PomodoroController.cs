using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Pomodoro.Commands;
using STLMS.Application.Pomodoro.Queries;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/pomodoro/sessions")]
public class PomodoroController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("TIMERS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        var sessions = await mediator.SendAsync(new GetPomodoroSessionsQuery(currentUser.UserId!.Value), ct);
        return Ok(sessions);
    }

    [RequirePermission("TIMERS", "create")]
    [HttpPost]
    public async Task<IActionResult> StartSession(StartPomodoroSessionRequest request, CancellationToken ct)
    {
        var session = await mediator.SendAsync(
            new StartPomodoroSessionCommand(
                currentUser.UserId!.Value, request.WorkMinutes, request.ShortBreakMinutes, request.LongBreakMinutes, request.CyclesBeforeLongBreak),
            ct);
        return Ok(session);
    }

    [RequirePermission("TIMERS", "edit")]
    [HttpPost("{sessionId:guid}/phases")]
    public async Task<IActionResult> LogPhase(Guid sessionId, LogPomodoroPhaseRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PomodoroPhase>(request.Phase, ignoreCase: true, out var phase))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("phase", "Unknown phase.")]);
        }

        await mediator.SendAsync(
            new LogPomodoroPhaseCommand(currentUser.UserId!.Value, sessionId, phase, request.StartedAt, request.EndedAt, request.CompletedFully), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("TIMERS", "edit")]
    [HttpPost("{sessionId:guid}/end")]
    public async Task<IActionResult> EndSession(Guid sessionId, CancellationToken ct)
    {
        await mediator.SendAsync(new EndPomodoroSessionCommand(currentUser.UserId!.Value, sessionId), ct);
        return Ok(new { success = true });
    }
}
