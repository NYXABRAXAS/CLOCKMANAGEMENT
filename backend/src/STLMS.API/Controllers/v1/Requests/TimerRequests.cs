namespace STLMS.API.Controllers.v1.Requests;

public record CreateCountdownTimerRequest(string Label, int DurationSeconds, string SoundId);

public record SaveStopwatchLapRequest(int LapNumber, long LapDurationMs, long CumulativeDurationMs);

public record SaveStopwatchSessionRequest(
    string Label,
    DateTime StartedAt,
    DateTime EndedAt,
    long TotalDurationMs,
    IReadOnlyList<SaveStopwatchLapRequest> Laps);

public record StartPomodoroSessionRequest(int WorkMinutes, int ShortBreakMinutes, int LongBreakMinutes, int CyclesBeforeLongBreak);

public record LogPomodoroPhaseRequest(string Phase, DateTime StartedAt, DateTime EndedAt, bool CompletedFully);
