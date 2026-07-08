using STLMS.Application.CalendarEvents.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.CalendarEvents;

public static class CalendarEventMapping
{
    public static CalendarEventDto ToDto(CalendarEvent e) => new(
        e.Id, e.Title, e.Description, e.Location, e.Color, e.IsAllDay, e.StartAt, e.EndAt,
        e.RecurrenceFrequency.ToString(), e.RecurrenceInterval, e.RecurrenceDaysMask, e.RecurrenceEndDate);
}
