using STLMS.Domain.Enums;

namespace STLMS.Application.Common.Interfaces;

public record ExternalUserInfo(string ProviderUserId, string Email, string FirstName, string LastName);

public interface IExternalAuthValidator
{
    ExternalLoginProvider Provider { get; }
    Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default);
}
