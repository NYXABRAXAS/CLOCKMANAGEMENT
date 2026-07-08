using STLMS.Application.Common.Mediator;
using STLMS.Application.Habits.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Queries;

public record GetHabitsQuery(Guid UserId) : IRequest<IReadOnlyList<HabitDto>>;

public class GetHabitsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetHabitsQuery, IReadOnlyList<HabitDto>>
{
    public async Task<IReadOnlyList<HabitDto>> HandleAsync(GetHabitsQuery request, CancellationToken ct)
    {
        var habits = await uow.Repository<Habit>().FindAsync(h => h.UserId == request.UserId, ct);
        if (habits.Count == 0) return [];

        var habitIds = habits.Select(h => h.Id).ToHashSet();
        var logs = (await uow.Repository<HabitLog>().FindAsync(l => habitIds.Contains(l.HabitId) && l.Completed, ct))
            .GroupBy(l => l.HabitId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DateOnly>)g.Select(l => l.Date).ToList());

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return habits
            .OrderBy(h => h.Title)
            .Select(h => ToDto(h, logs.TryGetValue(h.Id, out var dates) ? dates : [], today))
            .ToList();
    }

    internal static HabitDto ToDto(Habit h, IReadOnlyList<DateOnly> completedDates, DateOnly today)
    {
        var (current, longest) = HabitStreakCalculator.Calculate(h.RepeatDaysMask, completedDates, today);
        return new HabitDto(h.Id, h.Title, h.Description, h.Emoji, h.Color, h.RepeatDaysMask, h.IsActive, current, longest, completedDates.Contains(today));
    }
}
