using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshTokenRaw, string? IpAddress, string? UserAgent) : IRequest<AuthResultDto>;

public class RefreshTokenCommandHandler(IUnitOfWork uow, ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> HandleAsync(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = tokenService.Hash(request.RefreshTokenRaw);
        var existing = await uow.Repository<RefreshToken>().SingleOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (existing is null || existing.Revoked || existing.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("Session expired. Please log in again.");
        }

        var user = await uow.Repository<User>().GetByIdAsync(existing.UserId, ct)
            ?? throw new UnauthorizedAppException("Session expired. Please log in again.");

        if (!user.IsActive) throw new ForbiddenException("This account has been disabled.");

        var auth = await LoginCommandHandler.IssueAuthResultAsync(uow, tokenService, user, existing.RememberMe, request.IpAddress, request.UserAgent, ct);

        existing.Revoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = tokenService.Hash(auth.Tokens.RefreshTokenRaw);
        uow.Repository<RefreshToken>().Update(existing);

        var oldSession = await uow.Repository<UserSession>().SingleOrDefaultAsync(s => s.RefreshTokenHash == hash, ct);
        if (oldSession is not null)
        {
            oldSession.Revoked = true;
            uow.Repository<UserSession>().Update(oldSession);
        }

        await uow.SaveChangesAsync(ct);
        return auth;
    }
}
