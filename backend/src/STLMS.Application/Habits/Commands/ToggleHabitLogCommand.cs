using STLMS.Application.Achievements;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Habits.Dtos;
using STLMS.Application.Habits.Queries;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Habits.Commands;

public record ToggleHabitLogCommand(Guid UserId, Guid HabitId, DateOnly Date, bool Completed) : IRequest<ToggleHabitLogResult>;

public class ToggleHabitLogCommandHandler(IUnitOfWork uow) : IRequestHandler<ToggleHabitLogCommand, ToggleHabitLogResult>
{
    public async Task<ToggleHabitLogResult> HandleAsync(ToggleHabitLogCommand request, CancellationToken ct)
    {
        var habit = await uow.Repository<Habit>().GetByIdAsync(request.HabitId, ct);
        if (habit is null || habit.UserId != request.UserId) throw new NotFoundException("Habit", request.HabitId);

        var existingLog = await uow.Repository<HabitLog>()
            .SingleOrDefaultAsync(l => l.HabitId == request.HabitId && l.Date == request.Date, ct);

        if (existingLog is null)
        {
            await uow.Repository<HabitLog>().AddAsync(
                new HabitLog { HabitId = habit.Id, UserId = request.UserId, Date = request.Date, Completed = request.Completed }, ct);
        }
        else
        {
            existingLog.Completed = request.Completed;
            uow.Repository<HabitLog>().Update(existingLog);
        }
        await uow.SaveChangesAsync(ct);

        var newlyEarned = new List<string>();
        if (request.Completed)
        {
            newlyEarned.AddRange(await EvaluateAchievementsAsync(uow, habit, request.UserId, request.Date, ct));
        }

        var completedDates = (await uow.Repository<HabitLog>().FindAsync(l => l.HabitId == habit.Id && l.Completed, ct))
            .Select(l => l.Date).ToList();
        var dto = GetHabitsQueryHandler.ToDto(habit, completedDates, DateOnly.FromDateTime(DateTime.UtcNow));

        return new ToggleHabitLogResult(dto, newlyEarned);
    }

    private static async Task<List<string>> EvaluateAchievementsAsync(IUnitOfWork uow, Habit habit, Guid userId, DateOnly date, CancellationToken ct)
    {
        var awarded = new List<string>();

        var totalCompletedCheckIns = (await uow.Repository<HabitLog>().FindAsync(l => l.UserId == userId && l.Completed, ct)).Count;
        if (totalCompletedCheckIns == 1 && await AchievementAwarder.AwardIfNotAlreadyAsync(uow, userId, AchievementCodes.HabitFirstCheckIn, ct))
        {
            awarded.Add(AchievementCodes.HabitFirstCheckIn);
        }
        if (totalCompletedCheckIns >= 100 && await AchievementAwarder.AwardIfNotAlreadyAsync(uow, userId, AchievementCodes.HabitCheckIns100, ct))
        {
            awarded.Add(AchievementCodes.HabitCheckIns100);
        }

        var completedDates = (await uow.Repository<HabitLog>().FindAsync(l => l.HabitId == habit.Id && l.Completed, ct))
            .Select(l => l.Date).ToList();
        var (current, _) = HabitStreakCalculator.Calculate(habit.RepeatDaysMask, completedDates, date);

        if (current >= 7 && await AchievementAwarder.AwardIfNotAlreadyAsync(uow, userId, AchievementCodes.HabitStreak7, ct))
        {
            awarded.Add(AchievementCodes.HabitStreak7);
        }
        if (current >= 30 && await AchievementAwarder.AwardIfNotAlreadyAsync(uow, userId, AchievementCodes.HabitStreak30, ct))
        {
            awarded.Add(AchievementCodes.HabitStreak30);
        }

        if (awarded.Count > 0) await uow.SaveChangesAsync(ct);
        return awarded;
    }
}
