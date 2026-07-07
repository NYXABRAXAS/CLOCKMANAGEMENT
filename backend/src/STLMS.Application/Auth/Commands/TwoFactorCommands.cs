using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Auth.Commands;

public record SetupTwoFactorCommand(Guid UserId) : IRequest<SetupTwoFactorResult>;
public record SetupTwoFactorResult(string Secret, string QrCodePngBase64);

public class SetupTwoFactorCommandHandler(IUnitOfWork uow, ITotpService totp, IEncryptionService encryption)
    : IRequestHandler<SetupTwoFactorCommand, SetupTwoFactorResult>
{
    public async Task<SetupTwoFactorResult> HandleAsync(SetupTwoFactorCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);
        if (user.TwoFactorEnabled) throw new ConflictException("Two-factor authentication is already enabled.");

        var secret = totp.GenerateSecret();
        // Stored but not yet "active" - TwoFactorEnabled flips to true only once a real code is
        // confirmed via VerifyTwoFactorSetupCommand, so a half-finished setup can't accidentally
        // lock the user out.
        user.TotpSecretEncrypted = encryption.Encrypt(secret);
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        var qr = totp.GenerateQrCodePngBase64(secret, user.Email, "STLMS");
        return new SetupTwoFactorResult(secret, qr);
    }
}

public record VerifyTwoFactorSetupCommand(Guid UserId, string Code) : IRequest<bool>;

public class VerifyTwoFactorSetupCommandValidator : AbstractValidator<VerifyTwoFactorSetupCommand>
{
    public VerifyTwoFactorSetupCommandValidator() => RuleFor(x => x.Code).NotEmpty().Length(6);
}

public class VerifyTwoFactorSetupCommandHandler(IUnitOfWork uow, ITotpService totp, IEncryptionService encryption)
    : IRequestHandler<VerifyTwoFactorSetupCommand, bool>
{
    public async Task<bool> HandleAsync(VerifyTwoFactorSetupCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);
        if (string.IsNullOrEmpty(user.TotpSecretEncrypted)) throw new ConflictException("Call setup before verifying.");

        if (!totp.VerifyCode(encryption.Decrypt(user.TotpSecretEncrypted), request.Code))
        {
            throw new UnauthorizedAppException("Invalid authentication code.");
        }

        user.TwoFactorEnabled = true;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public record DisableTwoFactorCommand(Guid UserId, string Code) : IRequest<bool>;

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator() => RuleFor(x => x.Code).NotEmpty().Length(6);
}

public class DisableTwoFactorCommandHandler(IUnitOfWork uow, ITotpService totp, IEncryptionService encryption)
    : IRequestHandler<DisableTwoFactorCommand, bool>
{
    public async Task<bool> HandleAsync(DisableTwoFactorCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);
        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TotpSecretEncrypted))
        {
            throw new ConflictException("Two-factor authentication is not enabled.");
        }

        if (!totp.VerifyCode(encryption.Decrypt(user.TotpSecretEncrypted), request.Code))
        {
            throw new UnauthorizedAppException("Invalid authentication code.");
        }

        user.TwoFactorEnabled = false;
        user.TotpSecretEncrypted = null;
        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
