using FluentValidation;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record LoginCommand(string Email, string Password, bool RememberMe, string? IpAddress, string? UserAgent)
    : IRequest<LoginResult>;

/// <summary>Either Auth is populated (login succeeded outright) or TwoFactorChallengeToken is
/// (password was correct but a TOTP code is still required via VerifyTwoFactorLoginCommand).</summary>
public record LoginResult(AuthResultDto? Auth, string? TwoFactorChallengeToken);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICacheService cache,
    IAuditService audit) : IRequestHandler<LoginCommand, LoginResult>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TwoFactorChallengeTtl = TimeSpan.FromMinutes(5);

    public async Task<LoginResult> HandleAsync(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await uow.Repository<User>().SingleOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || user.PasswordHash is null)
        {
            await audit.LogAsync("LOGIN_FAILED", "USER", description: $"No account for {email}", ct: ct);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            await audit.LogAsync("LOGIN_FAILED", "USER", user.Id, description: "Account disabled", ct: ct);
            throw new ForbiddenException("This account has been disabled.");
        }

        if (user.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            await audit.LogAsync("LOGIN_FAILED", "USER", user.Id, description: "Account locked", ct: ct);
            throw new ForbiddenException("Account temporarily locked due to failed login attempts. Try again later.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockedUntil = DateTime.UtcNow.Add(LockDuration);
                user.FailedLoginAttempts = 0;
            }
            uow.Repository<User>().Update(user);
            await uow.SaveChangesAsync(ct);
            await audit.LogAsync("LOGIN_FAILED", "USER", user.Id, description: "Invalid password", ct: ct);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;

        if (user.TwoFactorEnabled)
        {
            var challengeToken = Guid.NewGuid().ToString("N");
            await cache.SetAsync($"2fa-challenge:{challengeToken}", user.Id, TwoFactorChallengeTtl, ct);
            uow.Repository<User>().Update(user);
            await uow.SaveChangesAsync(ct);
            return new LoginResult(null, challengeToken);
        }

        var auth = await IssueAuthResultAsync(uow, tokenService, user, request.RememberMe, request.IpAddress, request.UserAgent, ct);
        user.LastLoginAt = DateTime.UtcNow;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync("LOGIN", "USER", user.Id, ct: ct);

        return new LoginResult(auth, null);
    }

    internal static async Task<AuthResultDto> IssueAuthResultAsync(
        IUnitOfWork uow, ITokenService tokenService, User user, bool rememberMe, string? ipAddress, string? userAgent, CancellationToken ct)
    {
        // Religion is a nav property, never eager-loaded by the repository - without this, every
        // login/refresh/2FA/external-login response would report religionCode as null regardless
        // of what's actually stored (only GetCurrentUserQuery used to do this lookup).
        if (user.ReligionId is { } religionId)
        {
            user.Religion = await uow.Repository<Religion>().GetByIdAsync(religionId, ct);
        }

        var (roles, permissions) = await UserAccessLoader.LoadAsync(uow, user.Id, ct);

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, roles, permissions);
        var (refreshRaw, refreshHash) = tokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.Add(rememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7));
        var csrfToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        await uow.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpiresAt,
            RememberMe = rememberMe,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        }, ct);

        await uow.Repository<UserSession>().AddAsync(new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = refreshHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = DeviceNameFromUserAgent(userAgent),
        }, ct);

        await uow.SaveChangesAsync(ct);

        var tokens = new IssuedTokens(accessToken, tokenService.AccessTokenMinutes * 60, refreshRaw, refreshExpiresAt, csrfToken);
        var profile = AuthMapping.ToProfileDto(user, roles, permissions);
        return new AuthResultDto(profile, tokens, false);
    }

    private static string? DeviceNameFromUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return null;
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)) return "Mobile device";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows device";
        if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase)) return "Mac device";
        return "Unknown device";
    }
}
