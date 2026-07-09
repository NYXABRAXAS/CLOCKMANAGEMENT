using Moq;
using STLMS.Application.Admin.Roles.Commands;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Tests.TestDoubles;
using STLMS.Domain.Entities;
using Xunit;

namespace STLMS.Application.Tests.Admin;

public class SetRolePermissionCommandTests
{
    private readonly FakeUnitOfWork _uow = new();
    private readonly Mock<IAuditService> _auditService = new();

    private SetRolePermissionCommandHandler BuildSut() => new(_uow, _auditService.Object);

    [Fact]
    public async Task Handle_TargetingSuperAdminRole_ThrowsConflict()
    {
        var superAdmin = new Role { Code = RoleCodes.SuperAdmin, Name = "Super Admin" };
        var permission = new Permission { Module = "USERS", Action = "view" };
        _uow.FakeRepository<Role>().Items.Add(superAdmin);
        _uow.FakeRepository<Permission>().Items.Add(permission);

        await Assert.ThrowsAsync<ConflictException>(
            () => BuildSut().HandleAsync(new SetRolePermissionCommand(Guid.NewGuid(), superAdmin.Id, permission.Id, false), CancellationToken.None));

        // Nothing should have been mutated by a rejected attempt.
        Assert.Empty(_uow.FakeRepository<RolePermission>().Items);
    }

    [Fact]
    public async Task Handle_GrantingToNonSuperAdminRole_AddsRolePermission()
    {
        var role = new Role { Code = RoleCodes.StandardUser, Name = "Standard User" };
        var permission = new Permission { Module = "USERS", Action = "view" };
        _uow.FakeRepository<Role>().Items.Add(role);
        _uow.FakeRepository<Permission>().Items.Add(permission);

        var result = await BuildSut().HandleAsync(new SetRolePermissionCommand(Guid.NewGuid(), role.Id, permission.Id, true), CancellationToken.None);

        Assert.True(result);
        var grant = Assert.Single(_uow.FakeRepository<RolePermission>().Items);
        Assert.Equal(role.Id, grant.RoleId);
        Assert.Equal(permission.Id, grant.PermissionId);
    }

    [Fact]
    public async Task Handle_RevokingAnExistingGrant_RemovesRolePermission()
    {
        var role = new Role { Code = RoleCodes.StandardUser, Name = "Standard User" };
        var permission = new Permission { Module = "USERS", Action = "view" };
        _uow.FakeRepository<Role>().Items.Add(role);
        _uow.FakeRepository<Permission>().Items.Add(permission);
        _uow.FakeRepository<RolePermission>().Items.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });

        await BuildSut().HandleAsync(new SetRolePermissionCommand(Guid.NewGuid(), role.Id, permission.Id, false), CancellationToken.None);

        Assert.Empty(_uow.FakeRepository<RolePermission>().Items);
    }

    [Fact]
    public async Task Handle_GrantingAlreadyGrantedPermission_IsANoOpAndDoesNotDuplicate()
    {
        var role = new Role { Code = RoleCodes.StandardUser, Name = "Standard User" };
        var permission = new Permission { Module = "USERS", Action = "view" };
        _uow.FakeRepository<Role>().Items.Add(role);
        _uow.FakeRepository<Permission>().Items.Add(permission);
        _uow.FakeRepository<RolePermission>().Items.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });

        await BuildSut().HandleAsync(new SetRolePermissionCommand(Guid.NewGuid(), role.Id, permission.Id, true), CancellationToken.None);

        Assert.Single(_uow.FakeRepository<RolePermission>().Items);
    }
}
