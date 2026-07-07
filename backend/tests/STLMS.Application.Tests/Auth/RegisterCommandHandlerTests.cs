using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using STLMS.Application.Auth.Commands;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Tests.TestDoubles;
using STLMS.Domain.Entities;
using STLMS.Infrastructure.Identity;
using Xunit;

namespace STLMS.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly FakeUnitOfWork _uow = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IHostEnvironment> _env = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["WebUrl"] = "http://localhost:5173" }).Build();

    public RegisterCommandHandlerTests()
    {
        _uow.FakeRepository<Role>().Items.Add(new Role { Code = RoleCodes.StandardUser, Name = "Standard User" });
        _env.Setup(e => e.EnvironmentName).Returns(Environments.Development);
    }

    private RegisterCommandHandler BuildSut() => new(_uow, new PasswordHasher(), _emailSender.Object, _env.Object, _configuration);

    [Fact]
    public async Task Handle_CreatesUserAndAssignsTheDefaultRole()
    {
        _emailSender.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await BuildSut().HandleAsync(new RegisterCommand("Ada", "Lovelace", "Ada@Example.com", "Sup3r$ecret"), CancellationToken.None);

        var user = _uow.FakeRepository<User>().Items.Single();
        Assert.Equal("ada@example.com", user.Email); // normalized to lowercase
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual("Sup3r$ecret", user.PasswordHash);

        var userRole = _uow.FakeRepository<UserRole>().Items.Single();
        Assert.Equal(user.Id, userRole.UserId);

        Assert.Equal(user.Id, result.UserId);
        Assert.True(result.VerificationEmailSent);
        Assert.Null(result.DevOnlyVerificationToken); // not exposed when the email actually sent
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ThrowsConflict()
    {
        _uow.FakeRepository<User>().Items.Add(new User { Email = "ada@example.com", FirstName = "Ada", LastName = "L" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            BuildSut().HandleAsync(new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Sup3r$ecret"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEmailDeliveryFails_ExposesDevOnlyTokenInDevelopment()
    {
        _emailSender.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await BuildSut().HandleAsync(new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Sup3r$ecret"), CancellationToken.None);

        Assert.False(result.VerificationEmailSent);
        Assert.NotNull(result.DevOnlyVerificationToken);
    }

    [Fact]
    public async Task Handle_WhenEmailDeliveryFails_DoesNotExposeDevOnlyTokenOutsideDevelopment()
    {
        _env.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        _emailSender.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await BuildSut().HandleAsync(new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Sup3r$ecret"), CancellationToken.None);

        Assert.Null(result.DevOnlyVerificationToken);
    }
}
