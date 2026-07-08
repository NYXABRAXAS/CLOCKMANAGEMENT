namespace STLMS.API.Controllers.v1.Requests;

public record MedicineTimeRequest(int Hour, int Minute);

public record CreateMedicineRequest(
    string Name, string? Dosage, string? Notes, DateOnly StartDate, DateOnly? EndDate, int RepeatDaysMask, IReadOnlyList<MedicineTimeRequest> Times);

public record UpdateMedicineRequest(
    string Name, string? Dosage, string? Notes, DateOnly StartDate, DateOnly? EndDate, int RepeatDaysMask, bool IsActive,
    IReadOnlyList<MedicineTimeRequest> Times);

public record LogMedicineDoseRequest(DateOnly ScheduledDate, int ScheduledHour, int ScheduledMinute, string Status);

public record CreateHabitRequest(string Title, string? Description, string? Emoji, string? Color, int RepeatDaysMask);

public record UpdateHabitRequest(string Title, string? Description, string? Emoji, string? Color, int RepeatDaysMask, bool IsActive);

public record ToggleHabitLogRequest(DateOnly Date, bool Completed);

public record SaveSleepLogRequest(DateOnly Date, DateTime BedTime, DateTime WakeTime, string? Quality, string? Notes);
