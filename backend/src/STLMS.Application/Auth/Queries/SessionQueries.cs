using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Queries;

public record GetSessionsQuery(Guid UserId, string? CurrentRefreshTokenHash) : IRequest<IReadOnlyList<SessionDto>>;

public class GetSessionsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetSessionsQuery, IReadOnlyList<SessionDto>>
{
    public Task<IReadOnlyList<SessionDto>> HandleAsync(GetSessionsQuery request, CancellationToken ct)
    {
        // Synchronous ToList(), not EF's ToListAsync() - see the note on UserAccessLoader for why
        // (a handful of rows per user; keeps this testable against plain in-memory fakes).
        var sessions = uow.Repository<UserSession>().Query()
            .Where(s => s.UserId == request.UserId && !s.Revoked)
            .OrderByDescending(s => s.LastActiveAt)
            .ToList();

        IReadOnlyList<SessionDto> dtos = sessions
            .Select(s => new SessionDto(s.Id, s.DeviceName, s.IpAddress, s.LastActiveAt, s.RefreshTokenHash == request.CurrentRefreshTokenHash))
            .ToList();
        return Task.FromResult(dtos);
    }
}

public record RevokeSessionCommand(Guid UserId, Guid SessionId) : IRequest<bool>;

public class RevokeSessionCommandHandler(IUnitOfWork uow) : IRequestHandler<RevokeSessionCommand, bool>
{
    public async Task<bool> HandleAsync(RevokeSessionCommand request, CancellationToken ct)
    {
        var session = await uow.Repository<UserSession>().SingleOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.Revoked = true;
        uow.Repository<UserSession>().Update(session);

        var token = await uow.Repository<RefreshToken>().SingleOrDefaultAsync(rt => rt.TokenHash == session.RefreshTokenHash, ct);
        if (token is not null)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
            uow.Repository<RefreshToken>().Update(token);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
