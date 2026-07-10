using System.Security.Cryptography;
using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string? TimezoneId = null)
    : IRequest<RegisterResult>;

public record RegisterResult(Guid UserId, string Email, bool VerificationEmailSent, string? DevOnlyVerificationToken);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least one symbol.");
    }
}

public class RegisterCommandHandler(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    Microsoft.Extensions.Hosting.IHostEnvironment env,
    Microsoft.Extensions.Configuration.IConfiguration configuration) : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> HandleAsync(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await uow.Repository<User>().SingleOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null) throw new ConflictException("An account with this email already exists.");

        var defaultRole = await uow.Repository<Role>().SingleOrDefaultAsync(r => r.Code == RoleCodes.StandardUser, ct)
            ?? throw new InvalidOperationException("Default role STANDARD_USER is not seeded.");

        var rawToken = GenerateToken();
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            EmailVerified = false,
            EmailVerificationTokenHash = Hash(rawToken),
            EmailVerificationExpiresAt = DateTime.UtcNow.AddHours(24),
            TimezoneId = ResolveTimezone(request.TimezoneId),
        };
        await uow.Repository<User>().AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        await uow.Repository<UserRole>().AddAsync(new UserRole { UserId = user.Id, RoleId = defaultRole.Id }, ct);
        await uow.SaveChangesAsync(ct);

        var webUrl = configuration["WebUrl"] ?? "http://localhost:5173";
        var verifyLink = $"{webUrl}/verify-email/{rawToken}";
        var sent = await emailSender.SendAsync(
            user.Email,
            "Verify your STLMS account",
            $"<p>Hi {user.FirstName},</p><p>Welcome to the Smart Time &amp; Lifestyle Management System. Please verify your email:</p><p><a href=\"{verifyLink}\">{verifyLink}</a></p><p>This link expires in 24 hours.</p>",
            ct);

        // Dev-only escape hatch: if no SMTP is configured, this is the only way to actually get
        // the token without a real mailbox. Never included when delivery succeeded, and only
        // ever added in Development.
        var devToken = !sent && string.Equals(env.EnvironmentName, Microsoft.Extensions.Hosting.Environments.Development, StringComparison.OrdinalIgnoreCase) ? rawToken : null;

        return new RegisterResult(user.Id, user.Email, sent, devToken);
    }

    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private static string Hash(string raw) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));

    // Falls back to the User entity's own "UTC" default for anything unrecognized, rather than
    // rejecting registration over a bad timezone string - alarms/reminders being off by a few
    // hours until the user fixes it in Settings is far better than blocking signup entirely.
    private static string ResolveTimezone(string? timezoneId)
    {
        if (string.IsNullOrWhiteSpace(timezoneId)) return "UTC";
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return timezoneId;
        }
        catch (TimeZoneNotFoundException)
        {
            return "UTC";
        }
        catch (InvalidTimeZoneException)
        {
            return "UTC";
        }
    }
}
