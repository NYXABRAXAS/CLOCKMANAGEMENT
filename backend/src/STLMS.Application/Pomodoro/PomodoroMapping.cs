using STLMS.Application.Pomodoro.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.Pomodoro;

public static class PomodoroMapping
{
    public static PomodoroSessionDto ToDto(PomodoroSession s, IReadOnlyList<PomodoroLog>? logs = null) => new(
        s.Id, s.WorkMinutes, s.ShortBreakMinutes, s.LongBreakMinutes, s.CyclesBeforeLongBreak, s.StartedAt, s.EndedAt,
        (logs ?? []).OrderBy(l => l.StartedAt)
            .Select(l => new PomodoroLogDto(l.Id, l.Phase.ToString(), l.StartedAt, l.EndedAt, l.CompletedFully))
            .ToList());
}
