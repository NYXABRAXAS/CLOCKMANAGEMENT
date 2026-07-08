using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Pomodoro.Commands;

public record LogPomodoroPhaseCommand(
    Guid UserId, Guid PomodoroSessionId, PomodoroPhase Phase, DateTime StartedAt, DateTime EndedAt, bool CompletedFully) : IRequest<bool>;

public class LogPomodoroPhaseCommandValidator : AbstractValidator<LogPomodoroPhaseCommand>
{
    public LogPomodoroPhaseCommandValidator()
    {
        RuleFor(x => x.EndedAt).GreaterThanOrEqualTo(x => x.StartedAt);
    }
}

public class LogPomodoroPhaseCommandHandler(IUnitOfWork uow) : IRequestHandler<LogPomodoroPhaseCommand, bool>
{
    public async Task<bool> HandleAsync(LogPomodoroPhaseCommand request, CancellationToken ct)
    {
        var session = await uow.Repository<PomodoroSession>().GetByIdAsync(request.PomodoroSessionId, ct);
        if (session is null || session.UserId != request.UserId) throw new NotFoundException("PomodoroSession", request.PomodoroSessionId);

        await uow.Repository<PomodoroLog>().AddAsync(
            new PomodoroLog
            {
                PomodoroSessionId = session.Id,
                Phase = request.Phase,
                StartedAt = request.StartedAt,
                EndedAt = request.EndedAt,
                CompletedFully = request.CompletedFully,
            },
            ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
