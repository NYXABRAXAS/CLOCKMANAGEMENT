using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Admin.Users.Queries;

public record ExportUsersQuery : IRequest<ExportFile>;

public class ExportUsersQueryHandler(IUnitOfWork uow, IExportService exportService) : IRequestHandler<ExportUsersQuery, ExportFile>
{
    public Task<ExportFile> HandleAsync(ExportUsersQuery request, CancellationToken ct)
    {
        var users = uow.Repository<User>().Query().OrderByDescending(u => u.CreatedAt).ToList();
        var userIds = users.Select(u => u.Id).ToHashSet();
        var roleAssignments = uow.Repository<UserRole>().Query().Where(ur => userIds.Contains(ur.UserId)).ToList();
        var roleNames = uow.Repository<Role>().Query().ToList().ToDictionary(r => r.Id, r => r.Code);
        var rolesByUser = roleAssignments.GroupBy(ur => ur.UserId).ToDictionary(g => g.Key, g => string.Join(";", g.Select(ur => roleNames.GetValueOrDefault(ur.RoleId, "?"))));

        var rows = users
            .Select(u => (IReadOnlyList<string>)new[]
            {
                u.Email, u.FirstName, u.LastName, rolesByUser.GetValueOrDefault(u.Id, ""), u.IsActive ? "Yes" : "No",
                u.SubscriptionStatus.ToString(), u.CreatedAt.ToString("yyyy-MM-dd"),
            })
            .ToList();

        var sheet = new ExportSheet("Users", ["Email", "First Name", "Last Name", "Roles", "Active", "Subscription", "Created At"], rows);
        return Task.FromResult(exportService.ToCsv("users", sheet));
    }
}
