using FluentValidation;
using STLMS.Application.Alarms.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

public record UpdateAlarmCommand(
    Guid UserId,
    Guid AlarmId,
    string Label,
    int Hour,
    int Minute,
    int RepeatDaysMask,
    bool IsEnabled,
    string SoundId,
    bool SnoozeEnabled,
    int SnoozeMinutes,
    AlarmChallengeType ChallengeType) : IRequest<AlarmDto>;

public class UpdateAlarmCommandValidator : AbstractValidator<UpdateAlarmCommand>
{
    public UpdateAlarmCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Hour).InclusiveBetween(0, 23);
        RuleFor(x => x.Minute).InclusiveBetween(0, 59);
        RuleFor(x => x.RepeatDaysMask).InclusiveBetween(0, AlarmDayMask.Everyday);
        RuleFor(x => x.SoundId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SnoozeMinutes).InclusiveBetween(1, 30);
    }
}

public class UpdateAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateAlarmCommand, AlarmDto>
{
    public async Task<AlarmDto> HandleAsync(UpdateAlarmCommand request, CancellationToken ct)
    {
        var alarm = await uow.Repository<Alarm>().GetByIdAsync(request.AlarmId, ct);
        if (alarm is null || alarm.UserId != request.UserId) throw new NotFoundException("Alarm", request.AlarmId);

        alarm.Label = request.Label.Trim();
        alarm.Hour = request.Hour;
        alarm.Minute = request.Minute;
        alarm.RepeatDaysMask = request.RepeatDaysMask;
        alarm.IsEnabled = request.IsEnabled;
        alarm.SoundId = request.SoundId;
        alarm.SnoozeEnabled = request.SnoozeEnabled;
        alarm.SnoozeMinutes = request.SnoozeMinutes;
        alarm.ChallengeType = request.ChallengeType;

        uow.Repository<Alarm>().Update(alarm);
        await uow.SaveChangesAsync(ct);
        return AlarmMapping.ToDto(alarm);
    }
}
