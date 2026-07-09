using Moq;
using STLMS.Application.Admin.Users.Commands;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Tests.TestDoubles;
using STLMS.Domain.Entities;
using Xunit;

namespace STLMS.Application.Tests.Admin;

public class AssignUserRoleCommandTests
{
    private readonly FakeUnitOfWork _uow = new();
    private readonly Mock<IAuditService> _auditService = new();

    private AssignUserRoleCommandHandler BuildSut() => new(_uow, _auditService.Object);

    [Fact]
    public async Task Handle_ActorTargetingSelf_ThrowsConflict()
    {
        var actorId = Guid.NewGuid();

        await Assert.ThrowsAsync<ConflictException>(
            () => BuildSut().HandleAsync(new AssignUserRoleCommand(actorId, actorId, RoleCodes.Admin), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnknownRoleCode_ThrowsValidation()
    {
        var target = new User { Email = "target@example.com", FirstName = "Target", LastName = "User" };
        _uow.FakeRepository<User>().Items.Add(target);

        await Assert.ThrowsAsync<ValidationException>(
            () => BuildSut().HandleAsync(new AssignUserRoleCommand(Guid.NewGuid(), target.Id, "NOT_A_REAL_ROLE"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReassigningRole_RemovesPreviousAssignmentAndAddsTheNewOne()
    {
        var target = new User { Email = "target@example.com", FirstName = "Target", LastName = "User" };
        var oldRole = new Role { Code = RoleCodes.StandardUser, Name = "Standard User" };
        var newRole = new Role { Code = RoleCodes.PremiumUser, Name = "Premium User" };
        _uow.FakeRepository<User>().Items.Add(target);
        _uow.FakeRepository<Role>().Items.Add(oldRole);
        _uow.FakeRepository<Role>().Items.Add(newRole);
        _uow.FakeRepository<UserRole>().Items.Add(new UserRole { UserId = target.Id, RoleId = oldRole.Id });

        var result = await BuildSut().HandleAsync(new AssignUserRoleCommand(Guid.NewGuid(), target.Id, RoleCodes.PremiumUser), CancellationToken.None);

        Assert.True(result);
        var assignment = Assert.Single(_uow.FakeRepository<UserRole>().Items);
        Assert.Equal(newRole.Id, assignment.RoleId);
    }

    [Fact]
    public async Task Handle_TargetDoesNotExist_ThrowsNotFound()
    {
        var role = new Role { Code = RoleCodes.Admin, Name = "Admin" };
        _uow.FakeRepository<Role>().Items.Add(role);

        await Assert.ThrowsAsync<NotFoundException>(
            () => BuildSut().HandleAsync(new AssignUserRoleCommand(Guid.NewGuid(), Guid.NewGuid(), RoleCodes.Admin), CancellationToken.None));
    }
}
