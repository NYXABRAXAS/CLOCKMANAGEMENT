using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Enums;

namespace STLMS.Infrastructure.ExternalServices.Auth;

/// <summary>Validates a Microsoft Entra ID (Azure AD) ID token posted from the frontend's own
/// MSAL.js sign-in flow - same SPA + JWT-backend pattern as GoogleAuthValidator. Requires
/// OAuth:Microsoft:ClientId (the app registration's Application ID) to be set - without it, every
/// token is rejected.</summary>
public class MicrosoftAuthValidator : IExternalAuthValidator
{
    private const string Authority = "https://login.microsoftonline.com/common/v2.0";
    private readonly IConfiguration _configuration;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager;

    public MicrosoftAuthValidator(IConfiguration configuration)
    {
        _configuration = configuration;
        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{Authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
    }

    public ExternalLoginProvider Provider => ExternalLoginProvider.Microsoft;

    public async Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        var clientId = _configuration["OAuth:Microsoft:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("Microsoft login is not configured on this server.");
        }

        var openIdConfig = await _configManager.GetConfigurationAsync(ct);

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuers = [Authority, "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0"],
            ValidAudience = clientId,
            IssuerSigningKeys = openIdConfig.SigningKeys,
            ValidateLifetime = true,
        };

        var handler = new JwtSecurityTokenHandler();
        System.Security.Claims.ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(idToken, validationParameters, out _);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAppException($"Invalid Microsoft sign-in token: {ex.Message}");
        }

        var subject = principal.FindFirst("oid")?.Value ?? principal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAppException("Microsoft token is missing a subject claim.");
        var email = principal.FindFirst("email")?.Value ?? principal.FindFirst("preferred_username")?.Value
            ?? throw new UnauthorizedAppException("Microsoft token is missing an email claim.");
        var name = principal.FindFirst("name")?.Value ?? email;
        var parts = name.Split(' ', 2);

        return new ExternalUserInfo(subject, email, parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}
