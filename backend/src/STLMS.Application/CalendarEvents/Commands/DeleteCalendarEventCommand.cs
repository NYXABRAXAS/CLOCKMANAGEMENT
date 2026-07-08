using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CalendarEvents.Commands;

public record DeleteCalendarEventCommand(Guid UserId, Guid CalendarEventId) : IRequest<bool>;

public class DeleteCalendarEventCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteCalendarEventCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCalendarEventCommand request, CancellationToken ct)
    {
        var calendarEvent = await uow.Repository<CalendarEvent>().GetByIdAsync(request.CalendarEventId, ct);
        if (calendarEvent is null || calendarEvent.UserId != request.UserId) throw new NotFoundException("CalendarEvent", request.CalendarEventId);

        uow.Repository<CalendarEvent>().Remove(calendarEvent);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
