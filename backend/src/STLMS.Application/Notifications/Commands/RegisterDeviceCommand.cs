using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Notifications.Commands;

public record RegisterDeviceCommand(Guid UserId, string FcmToken, string? Platform) : IRequest<bool>;

public class RegisterDeviceCommandValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceCommandValidator()
    {
        RuleFor(x => x.FcmToken).NotEmpty().MaximumLength(4096);
    }
}

public class RegisterDeviceCommandHandler(IUnitOfWork uow) : IRequestHandler<RegisterDeviceCommand, bool>
{
    public async Task<bool> HandleAsync(RegisterDeviceCommand request, CancellationToken ct)
    {
        var existing = await uow.Repository<UserDevice>().FindAsync(d => d.UserId == request.UserId && d.FcmToken == request.FcmToken, ct);
        if (existing.Count > 0) return true;

        await uow.Repository<UserDevice>().AddAsync(
            new UserDevice { UserId = request.UserId, FcmToken = request.FcmToken, Platform = request.Platform }, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
