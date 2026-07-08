using STLMS.Application.Achievements.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Achievements.Queries;

public record GetUserAchievementsQuery(Guid UserId) : IRequest<IReadOnlyList<UserAchievementDto>>;

public class GetUserAchievementsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetUserAchievementsQuery, IReadOnlyList<UserAchievementDto>>
{
    public async Task<IReadOnlyList<UserAchievementDto>> HandleAsync(GetUserAchievementsQuery request, CancellationToken ct)
    {
        var earned = await uow.Repository<UserAchievement>().FindAsync(ua => ua.UserId == request.UserId, ct);
        if (earned.Count == 0) return [];

        var achievementIds = earned.Select(e => e.AchievementId).ToHashSet();
        var achievements = (await uow.Repository<Achievement>().FindAsync(a => achievementIds.Contains(a.Id), ct)).ToDictionary(a => a.Id);

        return earned
            .Where(e => achievements.ContainsKey(e.AchievementId))
            .OrderByDescending(e => e.EarnedAt)
            .Select(e =>
            {
                var a = achievements[e.AchievementId];
                return new UserAchievementDto(a.Code, a.Title, a.Description, a.Emoji, e.EarnedAt);
            })
            .ToList();
    }
}
