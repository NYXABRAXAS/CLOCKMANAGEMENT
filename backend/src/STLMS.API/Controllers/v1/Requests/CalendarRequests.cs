namespace STLMS.API.Controllers.v1.Requests;

public record CreateCalendarEventRequest(
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

public record UpdateCalendarEventRequest(
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

public record CreateEventCountdownRequest(string Title, DateOnly TargetDate, string? Emoji, string? Color);
