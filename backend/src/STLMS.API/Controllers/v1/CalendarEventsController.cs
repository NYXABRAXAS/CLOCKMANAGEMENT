using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.CalendarEvents.Commands;
using STLMS.Application.CalendarEvents.Queries;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calendar-events")]
public class CalendarEventsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    private static RecurrenceFrequency ParseFrequency(string value) =>
        Enum.TryParse<RecurrenceFrequency>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ValidationException([new FluentValidation.Results.ValidationFailure("recurrenceFrequency", "Unknown recurrence frequency.")]);

    [RequirePermission("CALENDAR", "view")]
    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] DateTime rangeStart, [FromQuery] DateTime rangeEnd, CancellationToken ct)
    {
        var occurrences = await mediator.SendAsync(new GetCalendarEventsQuery(currentUser.UserId!.Value, rangeStart, rangeEnd), ct);
        return Ok(occurrences);
    }

    [RequirePermission("CALENDAR", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateCalendarEventRequest request, CancellationToken ct)
    {
        var created = await mediator.SendAsync(
            new CreateCalendarEventCommand(
                currentUser.UserId!.Value, request.Title, request.Description, request.Location, request.Color, request.IsAllDay,
                request.StartAt, request.EndAt, ParseFrequency(request.RecurrenceFrequency), request.RecurrenceInterval,
                request.RecurrenceDaysMask, request.RecurrenceEndDate),
            ct);
        return Ok(created);
    }

    [RequirePermission("CALENDAR", "edit")]
    [HttpPut("{calendarEventId:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid calendarEventId, UpdateCalendarEventRequest request, CancellationToken ct)
    {
        var updated = await mediator.SendAsync(
            new UpdateCalendarEventCommand(
                currentUser.UserId!.Value, calendarEventId, request.Title, request.Description, request.Location, request.Color,
                request.IsAllDay, request.StartAt, request.EndAt, ParseFrequency(request.RecurrenceFrequency), request.RecurrenceInterval,
                request.RecurrenceDaysMask, request.RecurrenceEndDate),
            ct);
        return Ok(updated);
    }

    [RequirePermission("CALENDAR", "delete")]
    [HttpDelete("{calendarEventId:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid calendarEventId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteCalendarEventCommand(currentUser.UserId!.Value, calendarEventId), ct);
        return Ok(new { success = true });
    }
}
