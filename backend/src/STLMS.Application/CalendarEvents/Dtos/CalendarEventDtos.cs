namespace STLMS.Application.CalendarEvents.Dtos;

public record CalendarEventDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    string? Color,
    bool IsAllDay,
    DateTime StartAt,
    DateTime EndAt,
    string RecurrenceFrequency,
    int RecurrenceInterval,
    int RecurrenceDaysMask,
    DateTime? RecurrenceEndDate);

/// <summary>A single expanded occurrence of a (possibly recurring) event within a queried date
/// range - OccurrenceStart/End are the actual instance times; StartAt/EndAt on the underlying
/// event stay the series' original first occurrence.</summary>
public record CalendarEventOccurrenceDto(CalendarEventDto Event, DateTime OccurrenceStart, DateTime OccurrenceEnd);
