using FluentValidation;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record VerifyTwoFactorLoginCommand(string ChallengeToken, string Code, bool RememberMe, string? IpAddress, string? UserAgent)
    : IRequest<AuthResultDto>;

public class VerifyTwoFactorLoginCommandValidator : AbstractValidator<VerifyTwoFactorLoginCommand>
{
    public VerifyTwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.ChallengeToken).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class VerifyTwoFactorLoginCommandHandler(
    IUnitOfWork uow,
    ITokenService tokenService,
    ITotpService totp,
    IEncryptionService encryption,
    ICacheService cache,
    IAuditService audit) : IRequestHandler<VerifyTwoFactorLoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> HandleAsync(VerifyTwoFactorLoginCommand request, CancellationToken ct)
    {
        var cacheKey = $"2fa-challenge:{request.ChallengeToken}";
        var userId = await cache.GetAsync<Guid?>(cacheKey, ct);
        if (userId is null) throw new UnauthorizedAppException("This login attempt has expired. Please log in again.");

        var user = await uow.Repository<User>().GetByIdAsync(userId.Value, ct)
            ?? throw new UnauthorizedAppException("Invalid login attempt.");

        if (string.IsNullOrEmpty(user.TotpSecretEncrypted) || !totp.VerifyCode(encryption.Decrypt(user.TotpSecretEncrypted), request.Code))
        {
            await audit.LogAsync("LOGIN_FAILED", "USER", user.Id, description: "Invalid 2FA code", ct: ct);
            throw new UnauthorizedAppException("Invalid authentication code.");
        }

        await cache.RemoveAsync(cacheKey, ct);

        var auth = await LoginCommandHandler.IssueAuthResultAsync(uow, tokenService, user, request.RememberMe, request.IpAddress, request.UserAgent, ct);
        user.LastLoginAt = DateTime.UtcNow;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync("LOGIN", "USER", user.Id, description: "via 2FA", ct: ct);

        return auth;
    }
}
