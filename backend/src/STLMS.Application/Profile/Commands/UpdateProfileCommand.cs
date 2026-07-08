using FluentValidation;
using STLMS.Application.Auth;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Profile.Commands;

public record UpdateProfileCommand(Guid UserId, string FirstName, string LastName) : IRequest<UserProfileDto>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProfileCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
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
