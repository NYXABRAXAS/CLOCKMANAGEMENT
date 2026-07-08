namespace STLMS.Application.CountdownTimers.Dtos;

public record CountdownTimerDto(Guid Id, string Label, int DurationSeconds, string SoundId);
