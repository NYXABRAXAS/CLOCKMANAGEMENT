using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Alarms.Commands;
using STLMS.Application.Alarms.Queries;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/alarms")]
public class AlarmsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    private static AlarmChallengeType ParseChallengeType(string value) =>
        Enum.TryParse<AlarmChallengeType>(value, ignoreCase: true, out var parsed) ? parsed : AlarmChallengeType.None;

    [RequirePermission("ALARMS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetAlarms(CancellationToken ct)
    {
        var alarms = await mediator.SendAsync(new GetAlarmsQuery(currentUser.UserId!.Value), ct);
        return Ok(alarms);
    }

    [RequirePermission("ALARMS", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateAlarm(CreateAlarmRequest request, CancellationToken ct)
    {
        var alarm = await mediator.SendAsync(
            new CreateAlarmCommand(
                currentUser.UserId!.Value, request.Label, request.Hour, request.Minute, request.RepeatDaysMask,
                request.SoundId, request.SnoozeEnabled, request.SnoozeMinutes, ParseChallengeType(request.ChallengeType)),
            ct);
        return Ok(alarm);
    }

    [RequirePermission("ALARMS", "edit")]
    [HttpPut("{alarmId:guid}")]
    public async Task<IActionResult> UpdateAlarm(Guid alarmId, UpdateAlarmRequest request, CancellationToken ct)
    {
        var alarm = await mediator.SendAsync(
            new UpdateAlarmCommand(
                currentUser.UserId!.Value, alarmId, request.Label, request.Hour, request.Minute, request.RepeatDaysMask,
                request.IsEnabled, request.SoundId, request.SnoozeEnabled, request.SnoozeMinutes, ParseChallengeType(request.ChallengeType)),
            ct);
        return Ok(alarm);
    }

    [RequirePermission("ALARMS", "edit")]
    [HttpPost("{alarmId:guid}/toggle")]
    public async Task<IActionResult> ToggleAlarm(Guid alarmId, ToggleAlarmRequest request, CancellationToken ct)
    {
        var alarm = await mediator.SendAsync(new ToggleAlarmCommand(currentUser.UserId!.Value, alarmId, request.IsEnabled), ct);
        return Ok(alarm);
    }

    [RequirePermission("ALARMS", "edit")]
    [HttpPost("{alarmId:guid}/snooze")]
    public async Task<IActionResult> SnoozeAlarm(Guid alarmId, CancellationToken ct)
    {
        await mediator.SendAsync(new SnoozeAlarmCommand(currentUser.UserId!.Value, alarmId), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("ALARMS", "edit")]
    [HttpPost("{alarmId:guid}/dismiss")]
    public async Task<IActionResult> DismissAlarm(Guid alarmId, CancellationToken ct)
    {
        await mediator.SendAsync(new DismissAlarmCommand(currentUser.UserId!.Value, alarmId), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("ALARMS", "delete")]
    [HttpDelete("{alarmId:guid}")]
    public async Task<IActionResult> DeleteAlarm(Guid alarmId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteAlarmCommand(currentUser.UserId!.Value, alarmId), ct);
        return Ok(new { success = true });
    }
}
