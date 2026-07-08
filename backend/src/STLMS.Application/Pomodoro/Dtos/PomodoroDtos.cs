namespace STLMS.Application.Pomodoro.Dtos;

public record PomodoroLogDto(Guid Id, string Phase, DateTime StartedAt, DateTime EndedAt, bool CompletedFully);

public record PomodoroSessionDto(
    Guid Id,
    int WorkMinutes,
    int ShortBreakMinutes,
    int LongBreakMinutes,
    int CyclesBeforeLongBreak,
    DateTime StartedAt,
    DateTime? EndedAt,
    IReadOnlyList<PomodoroLogDto> Logs);
