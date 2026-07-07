using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;

    public int AccessTokenMinutes { get; }

    public TokenService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"] ?? "STLMS";
        _audience = configuration["Jwt:Audience"] ?? "STLMS.Client";
        AccessTokenMinutes = configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 15;

        var secret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            // Dev-only fallback so local `dotnet run` works without extra setup - production must
            // set Jwt:Secret via environment variable / user-secrets, never in committed config.
            secret = "stlms-dev-only-jwt-signing-secret-do-not-use-in-production-min-32-bytes";
        }
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("perm", p)));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string raw, string hash) GenerateRefreshToken()
    {
        var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        return (raw, Hash(raw));
    }

    public string Hash(string raw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
