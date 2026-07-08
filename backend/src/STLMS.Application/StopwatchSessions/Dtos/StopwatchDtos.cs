namespace STLMS.Application.StopwatchSessions.Dtos;

public record StopwatchLapDto(int LapNumber, long LapDurationMs, long CumulativeDurationMs);

public record StopwatchSessionDto(
    Guid Id,
    string Label,
    DateTime StartedAt,
    DateTime EndedAt,
    long TotalDurationMs,
    IReadOnlyList<StopwatchLapDto> Laps);
