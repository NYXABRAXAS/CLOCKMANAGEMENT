using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Achievements;

/// <summary>Shared "award once" helper any module can call after logging progress (Habit check-ins
/// today; Medicine adherence streaks, Pomodoro focus totals, etc. later) - callers still need to
/// SaveChangesAsync afterward.</summary>
public static class AchievementAwarder
{
    public static async Task<bool> AwardIfNotAlreadyAsync(IUnitOfWork uow, Guid userId, string achievementCode, CancellationToken ct)
    {
        var achievement = await uow.Repository<Achievement>().SingleOrDefaultAsync(a => a.Code == achievementCode, ct);
        if (achievement is null) return false;

        var alreadyEarned = await uow.Repository<UserAchievement>()
            .SingleOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id, ct);
        if (alreadyEarned is not null) return false;

        await uow.Repository<UserAchievement>().AddAsync(new UserAchievement { UserId = userId, AchievementId = achievement.Id }, ct);
        return true;
    }
}
