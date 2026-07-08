using FluentValidation;
using STLMS.Application.CalendarEvents.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CalendarEvents.Commands;

public record UpdateCalendarEventCommand(
    Guid UserId,
    Guid CalendarEventId,
    string Title,
    string? Description,
    string? Location,
    string? Color,
    bool IsAllDay,
    DateTime StartAt,
    DateTime EndAt,
    RecurrenceFrequency RecurrenceFrequency,
    int RecurrenceInterval,
    int RecurrenceDaysMask,
    DateTime? RecurrenceEndDate) : IRequest<CalendarEventDto>;

public class UpdateCalendarEventCommandValidator : AbstractValidator<UpdateCalendarEventCommand>
{
    public UpdateCalendarEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndAt).GreaterThanOrEqualTo(x => x.StartAt);
        RuleFor(x => x.RecurrenceInterval).GreaterThanOrEqualTo(1);
    }
}

public class UpdateCalendarEventCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateCalendarEventCommand, CalendarEventDto>
{
    public async Task<CalendarEventDto> HandleAsync(UpdateCalendarEventCommand request, CancellationToken ct)
    {
        var calendarEvent = await uow.Repository<CalendarEvent>().GetByIdAsync(request.CalendarEventId, ct);
        if (calendarEvent is null || calendarEvent.UserId != request.UserId) throw new NotFoundException("CalendarEvent", request.CalendarEventId);

        calendarEvent.Title = request.Title.Trim();
        calendarEvent.Description = request.Description;
        calendarEvent.Location = request.Location;
        calendarEvent.Color = request.Color;
        calendarEvent.IsAllDay = request.IsAllDay;
        calendarEvent.StartAt = request.StartAt;
        calendarEvent.EndAt = request.EndAt;
        calendarEvent.RecurrenceFrequency = request.RecurrenceFrequency;
        calendarEvent.RecurrenceInterval = request.RecurrenceInterval;
        calendarEvent.RecurrenceDaysMask = request.RecurrenceDaysMask;
        calendarEvent.RecurrenceEndDate = request.RecurrenceEndDate;

        uow.Repository<CalendarEvent>().Update(calendarEvent);
        await uow.SaveChangesAsync(ct);
        return CalendarEventMapping.ToDto(calendarEvent);
    }
}
