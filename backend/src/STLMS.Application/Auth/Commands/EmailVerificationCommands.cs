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

public record VerifyEmailCommand(string Token) : IRequest<bool>;

public class VerifyEmailCommandHandler(IUnitOfWork uow) : IRequestHandler<VerifyEmailCommand, bool>
{
    public async Task<bool> HandleAsync(VerifyEmailCommand request, CancellationToken ct)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var user = await uow.Repository<User>().SingleOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash, ct);

        if (user is null || user.EmailVerificationExpiresAt is null || user.EmailVerificationExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("This verification link is invalid or has expired.");
        }

        user.EmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationExpiresAt = null;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public record ResendVerificationCommand(string Email) : IRequest<ResendVerificationResult>;
public record ResendVerificationResult(bool EmailSent, string? DevOnlyVerificationToken);

public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ResendVerificationCommandHandler(
    IUnitOfWork uow, IEmailSender emailSender, IHostEnvironment env, IConfiguration configuration)
    : IRequestHandler<ResendVerificationCommand, ResendVerificationResult>
{
    public async Task<ResendVerificationResult> HandleAsync(ResendVerificationCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await uow.Repository<User>().SingleOrDefaultAsync(u => u.Email == email, ct);

        // Same anti-enumeration stance as ForgotPassword: behave identically either way.
        if (user is null || user.EmailVerified) return new ResendVerificationResult(true, null);

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        user.EmailVerificationExpiresAt = DateTime.UtcNow.AddHours(24);
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        var webUrl = configuration["WebUrl"] ?? "http://localhost:5173";
        var verifyLink = $"{webUrl}/verify-email/{rawToken}";
        var sent = await emailSender.SendAsync(
            user.Email,
            "Verify your STLMS account",
            $"<p>Hi {user.FirstName},</p><p>Please verify your email:</p><p><a href=\"{verifyLink}\">{verifyLink}</a></p><p>This link expires in 24 hours.</p>",
            ct);

        return new ResendVerificationResult(true, !sent && string.Equals(env.EnvironmentName, Microsoft.Extensions.Hosting.Environments.Development, StringComparison.OrdinalIgnoreCase) ? rawToken : null);
    }
}
