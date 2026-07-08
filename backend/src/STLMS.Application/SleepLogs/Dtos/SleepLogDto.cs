namespace STLMS.Application.SleepLogs.Dtos;

public record SleepLogDto(Guid Id, DateOnly Date, DateTime BedTime, DateTime WakeTime, int DurationMinutes, string? Quality, string? Notes);
