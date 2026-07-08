namespace STLMS.Application.Medicines.Dtos;

public record MedicineTimeDto(int Hour, int Minute);

public record MedicineDto(
    Guid Id,
    string Name,
    string? Dosage,
    string? Notes,
    DateOnly StartDate,
    DateOnly? EndDate,
    int RepeatDaysMask,
    bool IsActive,
    IReadOnlyList<MedicineTimeDto> Times);

public record MedicineLogDto(Guid MedicineId, DateOnly ScheduledDate, int ScheduledHour, int ScheduledMinute, string Status);
