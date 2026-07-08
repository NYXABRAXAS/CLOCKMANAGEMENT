using STLMS.Application.Alarms.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.Alarms;

public static class AlarmMapping
{
    public static AlarmDto ToDto(Alarm a) => new(
        a.Id, a.Label, a.Hour, a.Minute, a.RepeatDaysMask, a.IsEnabled, a.SoundId, a.SnoozeEnabled, a.SnoozeMinutes, a.ChallengeType.ToString());
}
