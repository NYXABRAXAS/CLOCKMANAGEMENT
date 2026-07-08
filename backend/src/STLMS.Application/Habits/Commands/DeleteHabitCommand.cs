using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Commands;

public record DeleteHabitCommand(Guid UserId, Guid HabitId) : IRequest<bool>;

public class DeleteHabitCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteHabitCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteHabitCommand request, CancellationToken ct)
    {
        var habit = await uow.Repository<Habit>().GetByIdAsync(request.HabitId, ct);
        if (habit is null || habit.UserId != request.UserId) throw new NotFoundException("Habit", request.HabitId);

        uow.Repository<Habit>().Remove(habit);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
