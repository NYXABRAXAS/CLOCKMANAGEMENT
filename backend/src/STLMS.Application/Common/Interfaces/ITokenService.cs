namespace STLMS.Application.Common.Interfaces;

public record IssuedTokens(
    string AccessToken,
    int AccessTokenExpiresInSeconds,
    string RefreshTokenRaw,
    DateTime RefreshTokenExpiresAt,
    string CsrfToken);

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
    (string raw, string hash) GenerateRefreshToken();
    string Hash(string raw);
    int AccessTokenMinutes { get; }
}
