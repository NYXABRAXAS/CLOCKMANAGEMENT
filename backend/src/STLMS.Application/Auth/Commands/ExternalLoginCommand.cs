using FluentValidation;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record ExternalLoginCommand(ExternalLoginProvider Provider, string IdToken, string? IpAddress, string? UserAgent)
    : IRequest<AuthResultDto>;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator() => RuleFor(x => x.IdToken).NotEmpty();
}

public class ExternalLoginCommandHandler(
    IUnitOfWork uow, ITokenService tokenService, IEnumerable<IExternalAuthValidator> validators)
    : IRequestHandler<ExternalLoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> HandleAsync(ExternalLoginCommand request, CancellationToken ct)
    {
        var validator = validators.FirstOrDefault(v => v.Provider == request.Provider)
            ?? throw new ConflictException($"External login provider '{request.Provider}' is not configured.");

        var info = await validator.ValidateAsync(request.IdToken, ct);

        var existingLink = await uow.Repository<ExternalLogin>()
            .SingleOrDefaultAsync(el => el.Provider == request.Provider && el.ProviderUserId == info.ProviderUserId, ct);

        User user;
        if (existingLink is not null)
        {
            user = await uow.Repository<User>().GetByIdAsync(existingLink.UserId, ct)
                ?? throw new UnauthorizedAppException("Account no longer exists.");
        }
        else
        {
            var email = info.Email.Trim().ToLowerInvariant();
            var existingByEmail = await uow.Repository<User>().SingleOrDefaultAsync(u => u.Email == email, ct);

            if (existingByEmail is not null)
            {
                user = existingByEmail;
            }
            else
            {
                var defaultRole = await uow.Repository<Role>().SingleOrDefaultAsync(r => r.Code == RoleCodes.StandardUser, ct)
                    ?? throw new InvalidOperationException("Default role STANDARD_USER is not seeded.");

                user = new User
                {
                    Email = email,
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    EmailVerified = true, // the external provider already verified this address
                    PasswordHash = null,
                };
                await uow.Repository<User>().AddAsync(user, ct);
                await uow.SaveChangesAsync(ct);
                await uow.Repository<UserRole>().AddAsync(new UserRole { UserId = user.Id, RoleId = defaultRole.Id }, ct);
            }

            await uow.Repository<ExternalLogin>().AddAsync(new ExternalLogin
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = info.ProviderUserId,
            }, ct);
            await uow.SaveChangesAsync(ct);
        }

        if (!user.IsActive) throw new ForbiddenException("This account has been disabled.");

        var auth = await LoginCommandHandler.IssueAuthResultAsync(uow, tokenService, user, false, request.IpAddress, request.UserAgent, ct);
        user.LastLoginAt = DateTime.UtcNow;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        return auth;
    }
}
