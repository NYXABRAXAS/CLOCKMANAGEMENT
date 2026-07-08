using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Habits.Dtos;
using STLMS.Application.Habits.Queries;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Commands;

public record UpdateHabitCommand(
    Guid UserId, Guid HabitId, string Title, string? Description, string? Emoji, string? Color, int RepeatDaysMask, bool IsActive) : IRequest<HabitDto>;

public class UpdateHabitCommandValidator : AbstractValidator<UpdateHabitCommand>
{
    public UpdateHabitCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class UpdateHabitCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateHabitCommand, HabitDto>
{
    public async Task<HabitDto> HandleAsync(UpdateHabitCommand request, CancellationToken ct)
    {
        var habit = await uow.Repository<Habit>().GetByIdAsync(request.HabitId, ct);
        if (habit is null || habit.UserId != request.UserId) throw new NotFoundException("Habit", request.HabitId);

        habit.Title = request.Title.Trim();
        habit.Description = request.Description;
        habit.Emoji = request.Emoji;
        habit.Color = request.Color;
        habit.RepeatDaysMask = request.RepeatDaysMask;
        habit.IsActive = request.IsActive;

        uow.Repository<Habit>().Update(habit);
        await uow.SaveChangesAsync(ct);

        var completedDates = (await uow.Repository<HabitLog>().FindAsync(l => l.HabitId == habit.Id && l.Completed, ct))
            .Select(l => l.Date).ToList();
        return GetHabitsQueryHandler.ToDto(habit, completedDates, DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
