using STLMS.Application.Auth;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Profile.Commands;

public record UpdateProfilePhotoCommand(Guid UserId, Stream Content, string FileExtension) : IRequest<UserProfileDto>;

public class UpdateProfilePhotoCommandHandler(IUnitOfWork uow, IFileStorageService fileStorage)
    : IRequestHandler<UpdateProfilePhotoCommand, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(UpdateProfilePhotoCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);

        user.PhotoUrl = await fileStorage.SaveProfilePhotoAsync(request.UserId, request.Content, request.FileExtension, ct);
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        if (user.ReligionId is { } religionId)
        {
            user.Religion = await uow.Repository<Religion>().GetByIdAsync(religionId, ct);
        }

        var (roles, permissions) = await UserAccessLoader.LoadAsync(uow, user.Id, ct);
        return AuthMapping.ToProfileDto(user, roles, permissions);
    }
}
