namespace STLMS.API.Controllers.v1.Requests;

public record CreateAlarmRequest(
    string Label,
    int Hour,
    int Minute,
    int RepeatDaysMask,
    string SoundId,
    bool SnoozeEnabled,
    int SnoozeMinutes,
    string ChallengeType);

public record UpdateAlarmRequest(
    string Label,
    int Hour,
    int Minute,
    int RepeatDaysMask,
    bool IsEnabled,
    string SoundId,
    bool SnoozeEnabled,
    int SnoozeMinutes,
    string ChallengeType);

public record ToggleAlarmRequest(bool IsEnabled);
