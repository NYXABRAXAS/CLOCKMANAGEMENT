using STLMS.Application.Admin.Users.Dtos;
using STLMS.Application.Common.Dtos;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Queries;

public record GetUsersQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<AdminUserDto>>;

public class GetUsersQueryHandler(IUnitOfWork uow) : IRequestHandler<GetUsersQuery, PagedResult<AdminUserDto>>
{
    public Task<PagedResult<AdminUserDto>> HandleAsync(GetUsersQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = uow.Repository<User>().Query();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) || u.FirstName.ToLower().Contains(term) || u.LastName.ToLower().Contains(term));
        }

        var totalCount = query.Count();
        var users = query.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var userIds = users.Select(u => u.Id).ToHashSet();
        var roleAssignments = uow.Repository<UserRole>().Query().Where(ur => userIds.Contains(ur.UserId)).ToList();
        var roleIds = roleAssignments.Select(ur => ur.RoleId).ToHashSet();
        var roles = uow.Repository<Role>().Query().Where(r => roleIds.Contains(r.Id)).ToList().ToDictionary(r => r.Id, r => r.Code);
        var rolesByUser = roleAssignments
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(ur => roles.GetValueOrDefault(ur.RoleId, "?")).ToList());

        var items = users
            .Select(u => new AdminUserDto(
                u.Id, u.Email, u.FirstName, u.LastName, u.EmailVerified, u.IsActive, u.TwoFactorEnabled, u.SubscriptionStatus.ToString(),
                u.FailedLoginAttempts, u.LockedUntil, u.LastLoginAt, u.CreatedAt, rolesByUser.GetValueOrDefault(u.Id, [])))
            .ToList();

        return Task.FromResult(new PagedResult<AdminUserDto>(items, totalCount, page, pageSize));
    }
}
