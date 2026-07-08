using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Habits.Dtos;
using STLMS.Application.Habits.Queries;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Commands;

public record CreateHabitCommand(Guid UserId, string Title, string? Description, string? Emoji, string? Color, int RepeatDaysMask) : IRequest<HabitDto>;

public class CreateHabitCommandValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class CreateHabitCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateHabitCommand, HabitDto>
{
    public async Task<HabitDto> HandleAsync(CreateHabitCommand request, CancellationToken ct)
    {
        var habit = new Habit
        {
            UserId = request.UserId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Emoji = request.Emoji,
            Color = request.Color,
            RepeatDaysMask = request.RepeatDaysMask,
        };
        await uow.Repository<Habit>().AddAsync(habit, ct);
        await uow.SaveChangesAsync(ct);
        return GetHabitsQueryHandler.ToDto(habit, [], DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
