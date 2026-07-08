namespace STLMS.Application.Alarms.Dtos;

public record AlarmDto(
    Guid Id,
    string Label,
    int Hour,
    int Minute,
    int RepeatDaysMask,
    bool IsEnabled,
    string SoundId,
    bool SnoozeEnabled,
    int SnoozeMinutes,
    string ChallengeType);
