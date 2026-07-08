using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.SleepLogs.Commands;
using STLMS.Application.SleepLogs.Queries;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sleep-logs")]
public class SleepController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    private static SleepQuality? ParseQuality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<SleepQuality>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ValidationException([new FluentValidation.Results.ValidationFailure("quality", "Unknown sleep quality.")]);
    }

    [RequirePermission("HEALTH", "view")]
    [HttpGet]
    public async Task<IActionResult> GetLogs(CancellationToken ct)
    {
        var logs = await mediator.SendAsync(new GetSleepLogsQuery(currentUser.UserId!.Value), ct);
        return Ok(logs);
    }

    [RequirePermission("HEALTH", "create")]
    [HttpPost]
    public async Task<IActionResult> SaveLog(SaveSleepLogRequest request, CancellationToken ct)
    {
        var log = await mediator.SendAsync(
            new SaveSleepLogCommand(currentUser.UserId!.Value, request.Date, request.BedTime, request.WakeTime, ParseQuality(request.Quality), request.Notes),
            ct);
        return Ok(log);
    }

    [RequirePermission("HEALTH", "delete")]
    [HttpDelete("{sleepLogId:guid}")]
    public async Task<IActionResult> DeleteLog(Guid sleepLogId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteSleepLogCommand(currentUser.UserId!.Value, sleepLogId), ct);
        return Ok(new { success = true });
    }
}
