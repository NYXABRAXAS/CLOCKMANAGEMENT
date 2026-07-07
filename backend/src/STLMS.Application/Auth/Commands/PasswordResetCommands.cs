using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;
public record ForgotPasswordResult(bool EmailSent, string? DevOnlyResetToken);

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ForgotPasswordCommandHandler(
    IUnitOfWork uow, IEmailSender emailSender, IHostEnvironment env, IConfiguration configuration)
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    public async Task<ForgotPasswordResult> HandleAsync(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await uow.Repository<User>().SingleOrDefaultAsync(u => u.Email == email, ct);

        // Always behave the same way whether or not the account exists, so this endpoint can't be
        // used to enumerate registered emails.
        if (user is null) return new ForgotPasswordResult(true, null);

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenHash = Sha256(rawToken);
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        var webUrl = configuration["WebUrl"] ?? "http://localhost:5173";
        var resetLink = $"{webUrl}/reset-password/{rawToken}";
        var sent = await emailSender.SendAsync(
            user.Email,
            "Reset your STLMS password",
            $"<p>Hi {user.FirstName},</p><p>We received a request to reset your password. This link expires in 1 hour:</p><p><a href=\"{resetLink}\">{resetLink}</a></p><p>If you didn't request this, you can ignore this email.</p>",
            ct);

        return new ForgotPasswordResult(true, !sent && string.Equals(env.EnvironmentName, Microsoft.Extensions.Hosting.Environments.Development, StringComparison.OrdinalIgnoreCase) ? rawToken : null);
    }

    private static string Sha256(string raw) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least one symbol.");
    }
}

public class ResetPasswordCommandHandler(IUnitOfWork uow, IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> HandleAsync(ResetPasswordCommand request, CancellationToken ct)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var user = await uow.Repository<User>().SingleOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash, ct);

        if (user is null || user.PasswordResetExpiresAt is null || user.PasswordResetExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("This password reset link is invalid or has expired.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetExpiresAt = null;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        uow.Repository<User>().Update(user);

        // Force re-login everywhere - a leaked/reset password shouldn't leave old sessions valid.
        var tokens = await uow.Repository<RefreshToken>().FindAsync(rt => rt.UserId == user.Id && !rt.Revoked, ct);
        foreach (var t in tokens) { t.Revoked = true; t.RevokedAt = DateTime.UtcNow; uow.Repository<RefreshToken>().Update(t); }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
