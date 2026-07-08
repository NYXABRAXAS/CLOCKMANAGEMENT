using FluentValidation;
using STLMS.Application.CalendarEvents.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CalendarEvents.Commands;

public record CreateCalendarEventCommand(
    Guid UserId,
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

public class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndAt).GreaterThanOrEqualTo(x => x.StartAt);
        RuleFor(x => x.RecurrenceInterval).GreaterThanOrEqualTo(1);
    }
}

public class CreateCalendarEventCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateCalendarEventCommand, CalendarEventDto>
{
    public async Task<CalendarEventDto> HandleAsync(CreateCalendarEventCommand request, CancellationToken ct)
    {
        var calendarEvent = new CalendarEvent
        {
            UserId = request.UserId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Location = request.Location,
            Color = request.Color,
            IsAllDay = request.IsAllDay,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            RecurrenceFrequency = request.RecurrenceFrequency,
            RecurrenceInterval = request.RecurrenceInterval,
            RecurrenceDaysMask = request.RecurrenceDaysMask,
            RecurrenceEndDate = request.RecurrenceEndDate,
        };
        await uow.Repository<CalendarEvent>().AddAsync(calendarEvent, ct);
        await uow.SaveChangesAsync(ct);
        return CalendarEventMapping.ToDto(calendarEvent);
    }
}
