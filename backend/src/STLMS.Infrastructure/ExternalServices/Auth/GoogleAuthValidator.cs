using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Enums;

namespace STLMS.Infrastructure.ExternalServices.Auth;

/// <summary>Validates the Google ID token the frontend obtains via Google's own client-side SDK
/// (Google Identity Services) and posts to our API - this is the standard SPA + JWT-backend
/// pattern, distinct from ASP.NET Core's cookie-oriented OAuth handlers which target
/// server-rendered apps. Requires OAuth:Google:ClientId to be set to your real Google Cloud OAuth
/// client ID - without it, every token is rejected (fails closed, not open).</summary>
public class GoogleAuthValidator(IConfiguration configuration) : IExternalAuthValidator
{
    public ExternalLoginProvider Provider => ExternalLoginProvider.Google;

    public async Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        var clientId = configuration["OAuth:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("Google login is not configured on this server.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId],
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAppException($"Invalid Google sign-in token: {ex.Message}");
        }

        return new ExternalUserInfo(payload.Subject, payload.Email, payload.GivenName ?? payload.Email, payload.FamilyName ?? string.Empty);
    }
}
