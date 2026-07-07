using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record LogoutCommand(string? RefreshTokenRaw) : IRequest<bool>;

public class LogoutCommandHandler(IUnitOfWork uow, ITokenService tokenService) : IRequestHandler<LogoutCommand, bool>
{
    public async Task<bool> HandleAsync(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.RefreshTokenRaw)) return true;

        var hash = tokenService.Hash(request.RefreshTokenRaw);
        var token = await uow.Repository<RefreshToken>().SingleOrDefaultAsync(rt => rt.TokenHash == hash, ct);
        if (token is not null && !token.Revoked)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
            uow.Repository<RefreshToken>().Update(token);
        }

        var session = await uow.Repository<UserSession>().SingleOrDefaultAsync(s => s.RefreshTokenHash == hash, ct);
        if (session is not null && !session.Revoked)
        {
            session.Revoked = true;
            uow.Repository<UserSession>().Update(session);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
