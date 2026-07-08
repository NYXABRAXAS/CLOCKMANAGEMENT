using STLMS.Application.CalendarEvents.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CalendarEvents.Queries;

public record GetCalendarEventsQuery(Guid UserId, DateTime RangeStart, DateTime RangeEnd) : IRequest<IReadOnlyList<CalendarEventOccurrenceDto>>;

public class GetCalendarEventsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetCalendarEventsQuery, IReadOnlyList<CalendarEventOccurrenceDto>>
{
    public async Task<IReadOnlyList<CalendarEventOccurrenceDto>> HandleAsync(GetCalendarEventsQuery request, CancellationToken ct)
    {
        // Expanded in-memory (not via SQL) - a user's total event count is small, and recurrence
        // expansion involves loop/branch logic that's much safer to reason about client-side of
        // the query than to try to translate into SQL.
        var events = await uow.Repository<CalendarEvent>().FindAsync(e => e.UserId == request.UserId, ct);

        var occurrences = new List<CalendarEventOccurrenceDto>();
        foreach (var e in events)
        {
            var dto = CalendarEventMapping.ToDto(e);
            foreach (var (start, end) in CalendarRecurrenceExpander.Expand(e, request.RangeStart, request.RangeEnd))
            {
                occurrences.Add(new CalendarEventOccurrenceDto(dto, start, end));
            }
        }

        return occurrences.OrderBy(o => o.OccurrenceStart).ToList();
    }
}
