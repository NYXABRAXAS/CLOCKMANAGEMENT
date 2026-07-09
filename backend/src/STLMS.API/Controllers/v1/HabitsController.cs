using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Habits.Commands;
using STLMS.Application.Habits.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/habits")]
public class HabitsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("HEALTH", "view")]
    [HttpGet]
    public async Task<IActionResult> GetHabits(CancellationToken ct)
    {
        var habits = await mediator.SendAsync(new GetHabitsQuery(currentUser.UserId!.Value), ct);
        return Ok(habits);
    }

    [RequirePermission("HEALTH", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateHabit(CreateHabitRequest request, CancellationToken ct)
    {
        var habit = await mediator.SendAsync(
            new CreateHabitCommand(currentUser.UserId!.Value, request.Title, request.Description, request.Emoji, request.Color, request.RepeatDaysMask),
            ct);
        return Ok(habit);
    }

    [RequirePermission("HEALTH", "edit")]
    [HttpPut("{habitId:guid}")]
    public async Task<IActionResult> UpdateHabit(Guid habitId, UpdateHabitRequest request, CancellationToken ct)
    {
        var habit = await mediator.SendAsync(
            new UpdateHabitCommand(
                currentUser.UserId!.Value, habitId, request.Title, request.Description, request.Emoji, request.Color, request.RepeatDaysMask,
                request.IsActive),
            ct);
        return Ok(habit);
    }

    [RequirePermission("HEALTH", "edit")]
    [HttpPost("{habitId:guid}/log")]
    public async Task<IActionResult> ToggleLog(Guid habitId, ToggleHabitLogRequest request, CancellationToken ct)
    {
        var result = await mediator.SendAsync(
            new ToggleHabitLogCommand(currentUser.UserId!.Value, habitId, request.Date, request.Completed), ct);
        return Ok(result);
    }

    [RequirePermission("HEALTH", "delete")]
    [HttpDelete("{habitId:guid}")]
    public async Task<IActionResult> DeleteHabit(Guid habitId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteHabitCommand(currentUser.UserId!.Value, habitId), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("HEALTH", "view")]
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var file = await mediator.SendAsync(new ExportHabitLogsQuery(currentUser.UserId!.Value), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
