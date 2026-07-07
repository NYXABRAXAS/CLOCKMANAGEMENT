using Microsoft.Extensions.Configuration;
using Moq;
using STLMS.Application.Auth.Commands;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Tests.TestDoubles;
using STLMS.Domain.Entities;
using STLMS.Infrastructure.Identity;
using Xunit;

namespace STLMS.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly FakeUnitOfWork _uow = new();
    private readonly PasswordHasher _passwordHasher = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IAuditService> _audit = new();

    private readonly ITokenService _tokenService = new TokenService(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Secret"] = "unit-test-signing-secret-at-least-32-bytes-long" }).Build());

    private User AddUser(string password, bool active = true, bool twoFactor = false)
    {
        var user = new User
        {
            Email = "ada@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = active,
            TwoFactorEnabled = twoFactor,
            TotpSecretEncrypted = twoFactor ? "irrelevant-for-this-test" : null,
        };
        _uow.FakeRepository<User>().Items.Add(user);
        return user;
    }

    private LoginCommandHandler BuildSut() => new(_uow, _passwordHasher, _tokenService, _cache.Object, _audit.Object);

    [Fact]
    public async Task Handle_WithCorrectCredentials_IssuesTokensAndCreatesASession()
    {
        AddUser("Sup3r$ecret");

        var result = await BuildSut().HandleAsync(new LoginCommand("ada@example.com", "Sup3r$ecret", false, "127.0.0.1", "test-agent"), CancellationToken.None);

        Assert.NotNull(result.Auth);
        Assert.Null(result.TwoFactorChallengeToken);
        Assert.NotEmpty(result.Auth!.Tokens.AccessToken);
        Assert.Single(_uow.FakeRepository<RefreshToken>().Items);
        Assert.Single(_uow.FakeRepository<UserSession>().Items);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorized()
    {
        AddUser("Sup3r$ecret");

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            BuildSut().HandleAsync(new LoginCommand("ada@example.com", "wrong", false, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AfterFiveFailedAttempts_LocksTheAccount()
    {
        var user = AddUser("Sup3r$ecret");

        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
                BuildSut().HandleAsync(new LoginCommand("ada@example.com", "wrong", false, null, null), CancellationToken.None));
        }

        Assert.NotNull(user.LockedUntil);
        Assert.True(user.LockedUntil > DateTime.UtcNow);

        // Even the correct password is now rejected while locked.
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            BuildSut().HandleAsync(new LoginCommand("ada@example.com", "Sup3r$ecret", false, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ForADisabledAccount_ThrowsForbidden()
    {
        AddUser("Sup3r$ecret", active: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            BuildSut().HandleAsync(new LoginCommand("ada@example.com", "Sup3r$ecret", false, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTwoFactorIsEnabled_ReturnsAChallengeTokenInsteadOfTokens()
    {
        AddUser("Sup3r$ecret", twoFactor: true);
        _cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await BuildSut().HandleAsync(new LoginCommand("ada@example.com", "Sup3r$ecret", false, null, null), CancellationToken.None);

        Assert.Null(result.Auth);
        Assert.NotNull(result.TwoFactorChallengeToken);
        _cache.Verify(c => c.SetAsync(
            $"2fa-challenge:{result.TwoFactorChallengeToken}", It.IsAny<Guid>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ForAnUnknownEmail_ThrowsUnauthorizedNotNotFound()
    {
        // Deliberately the same exception/message as a wrong password - must not reveal whether
        // an account exists.
        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            BuildSut().HandleAsync(new LoginCommand("nobody@example.com", "whatever", false, null, null), CancellationToken.None));
    }
}
