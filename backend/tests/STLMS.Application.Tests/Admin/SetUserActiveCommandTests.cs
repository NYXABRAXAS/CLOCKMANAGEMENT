using Moq;
using STLMS.Application.Admin.Users.Commands;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Tests.TestDoubles;
using STLMS.Domain.Entities;
using Xunit;

namespace STLMS.Application.Tests.Admin;

public class SetUserActiveCommandTests
{
    private readonly FakeUnitOfWork _uow = new();
    private readonly Mock<IAuditService> _auditService = new();

    private SetUserActiveCommandHandler BuildSut() => new(_uow, _auditService.Object);

    [Fact]
    public async Task Handle_ActorTargetingSelf_ThrowsConflict_RegardlessOfTargetExisting()
    {
        var actorId = Guid.NewGuid();

        await Assert.ThrowsAsync<ConflictException>(() => BuildSut().HandleAsync(new SetUserActiveCommand(actorId, actorId, false), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeactivatingAnotherUser_UpdatesIsActiveAndLogsAudit()
    {
        var target = new User { Email = "target@example.com", FirstName = "Target", LastName = "User", IsActive = true };
        _uow.FakeRepository<User>().Items.Add(target);

        var result = await BuildSut().HandleAsync(new SetUserActiveCommand(Guid.NewGuid(), target.Id, false), CancellationToken.None);

        Assert.True(result);
        Assert.False(target.IsActive);
        _auditService.Verify(
            a => a.LogAsync("DEACTIVATE", "User", target.Id, It.IsAny<object>(), It.IsAny<object>(), null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TargetDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => BuildSut().HandleAsync(new SetUserActiveCommand(Guid.NewGuid(), Guid.NewGuid(), true), CancellationToken.None));
    }
}
