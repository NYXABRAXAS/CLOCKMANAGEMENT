using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserProfileDto>;

public class GetCurrentUserQueryHandler(IUnitOfWork uow) : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);

        // Religion is loaded via a second lookup instead of an EF Include() to keep Application
        // free of any EF-specific query composition.
        if (user.ReligionId is { } religionId)
        {
            user.Religion = await uow.Repository<Religion>().GetByIdAsync(religionId, ct);
        }

        var (roles, permissions) = await UserAccessLoader.LoadAsync(uow, user.Id, ct);
        return AuthMapping.ToProfileDto(user, roles, permissions);
    }
}
