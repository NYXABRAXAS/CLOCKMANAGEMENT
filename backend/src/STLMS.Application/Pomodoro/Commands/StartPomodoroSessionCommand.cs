using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Pomodoro.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Pomodoro.Commands;

public record StartPomodoroSessionCommand(
    Guid UserId, int WorkMinutes, int ShortBreakMinutes, int LongBreakMinutes, int CyclesBeforeLongBreak) : IRequest<PomodoroSessionDto>;

public class StartPomodoroSessionCommandValidator : AbstractValidator<StartPomodoroSessionCommand>
{
    public StartPomodoroSessionCommandValidator()
    {
        RuleFor(x => x.WorkMinutes).InclusiveBetween(1, 120);
        RuleFor(x => x.ShortBreakMinutes).InclusiveBetween(1, 60);
        RuleFor(x => x.LongBreakMinutes).InclusiveBetween(1, 120);
        RuleFor(x => x.CyclesBeforeLongBreak).InclusiveBetween(1, 12);
    }
}

public class StartPomodoroSessionCommandHandler(IUnitOfWork uow) : IRequestHandler<StartPomodoroSessionCommand, PomodoroSessionDto>
{
    public async Task<PomodoroSessionDto> HandleAsync(StartPomodoroSessionCommand request, CancellationToken ct)
    {
        var session = new PomodoroSession
        {
            UserId = request.UserId,
            WorkMinutes = request.WorkMinutes,
            ShortBreakMinutes = request.ShortBreakMinutes,
            LongBreakMinutes = request.LongBreakMinutes,
            CyclesBeforeLongBreak = request.CyclesBeforeLongBreak,
            StartedAt = DateTime.UtcNow,
        };
        await uow.Repository<PomodoroSession>().AddAsync(session, ct);
        await uow.SaveChangesAsync(ct);
        return PomodoroMapping.ToDto(session);
    }
}
