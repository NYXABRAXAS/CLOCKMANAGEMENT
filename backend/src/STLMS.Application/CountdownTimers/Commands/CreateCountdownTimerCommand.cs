using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.CountdownTimers.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.CountdownTimers.Commands;

public record CreateCountdownTimerCommand(Guid UserId, string Label, int DurationSeconds, string SoundId) : IRequest<CountdownTimerDto>;

public class CreateCountdownTimerCommandValidator : AbstractValidator<CreateCountdownTimerCommand>
{
    public CreateCountdownTimerCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationSeconds).InclusiveBetween(1, 24 * 60 * 60);
        RuleFor(x => x.SoundId).NotEmpty().MaximumLength(50);
    }
}

public class CreateCountdownTimerCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateCountdownTimerCommand, CountdownTimerDto>
{
    public async Task<CountdownTimerDto> HandleAsync(CreateCountdownTimerCommand request, CancellationToken ct)
    {
        var timer = new CountdownTimer
        {
            UserId = request.UserId,
            Label = request.Label.Trim(),
            DurationSeconds = request.DurationSeconds,
            SoundId = request.SoundId,
        };
        await uow.Repository<CountdownTimer>().AddAsync(timer, ct);
        await uow.SaveChangesAsync(ct);
        return new CountdownTimerDto(timer.Id, timer.Label, timer.DurationSeconds, timer.SoundId);
    }
}
