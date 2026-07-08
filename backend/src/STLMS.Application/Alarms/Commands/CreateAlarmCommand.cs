using FluentValidation;
using STLMS.Application.Alarms.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Alarms.Commands;

public record CreateAlarmCommand(
    Guid UserId,
    string Label,
    int Hour,
    int Minute,
    int RepeatDaysMask,
    string SoundId,
    bool SnoozeEnabled,
    int SnoozeMinutes,
    AlarmChallengeType ChallengeType) : IRequest<AlarmDto>;

public class CreateAlarmCommandValidator : AbstractValidator<CreateAlarmCommand>
{
    public CreateAlarmCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Hour).InclusiveBetween(0, 23);
        RuleFor(x => x.Minute).InclusiveBetween(0, 59);
        RuleFor(x => x.RepeatDaysMask).InclusiveBetween(0, AlarmDayMask.Everyday);
        RuleFor(x => x.SoundId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SnoozeMinutes).InclusiveBetween(1, 30);
    }
}

public class CreateAlarmCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateAlarmCommand, AlarmDto>
{
    public async Task<AlarmDto> HandleAsync(CreateAlarmCommand request, CancellationToken ct)
    {
        var alarm = new Alarm
        {
            UserId = request.UserId,
            Label = request.Label.Trim(),
            Hour = request.Hour,
            Minute = request.Minute,
            RepeatDaysMask = request.RepeatDaysMask,
            SoundId = request.SoundId,
            SnoozeEnabled = request.SnoozeEnabled,
            SnoozeMinutes = request.SnoozeMinutes,
            ChallengeType = request.ChallengeType,
        };
        await uow.Repository<Alarm>().AddAsync(alarm, ct);
        await uow.SaveChangesAsync(ct);
        return AlarmMapping.ToDto(alarm);
    }
}
