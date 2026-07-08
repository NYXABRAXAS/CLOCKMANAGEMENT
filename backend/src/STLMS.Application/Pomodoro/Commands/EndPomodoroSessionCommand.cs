using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Pomodoro.Commands;

public record EndPomodoroSessionCommand(Guid UserId, Guid PomodoroSessionId) : IRequest<bool>;

public class EndPomodoroSessionCommandHandler(IUnitOfWork uow) : IRequestHandler<EndPomodoroSessionCommand, bool>
{
    public async Task<bool> HandleAsync(EndPomodoroSessionCommand request, CancellationToken ct)
    {
        var session = await uow.Repository<PomodoroSession>().GetByIdAsync(request.PomodoroSessionId, ct);
        if (session is null || session.UserId != request.UserId) throw new NotFoundException("PomodoroSession", request.PomodoroSessionId);

        session.EndedAt = DateTime.UtcNow;
        uow.Repository<PomodoroSession>().Update(session);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
