using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.PrayerTimes.Commands;
using STLMS.Application.PrayerTimes.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/prayer-times")]
[RequirePermission("PRAYER_CENTER", "view")]
public class PrayerTimesController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTimes([FromQuery] DateOnly date, CancellationToken ct)
    {
        var times = await mediator.SendAsync(new GetPrayerTimesQuery(currentUser.UserId!.Value, date), ct);
        return Ok(times);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] DateOnly date, CancellationToken ct)
    {
        var logs = await mediator.SendAsync(new GetPrayerLogsQuery(currentUser.UserId!.Value, date), ct);
        return Ok(logs);
    }

    [RequirePermission("PRAYER_CENTER", "edit")]
    [HttpPost("log")]
    public async Task<IActionResult> LogPrayer(LogPrayerRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new LogPrayerCommand(currentUser.UserId!.Value, request.Date, request.PrayerName, request.Completed), ct);
        return Ok(new { success = true });
    }
}
