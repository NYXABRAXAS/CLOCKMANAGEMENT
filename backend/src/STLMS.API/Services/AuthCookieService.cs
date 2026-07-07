using STLMS.Application.Common.Interfaces;

namespace STLMS.API.Services;

/// <summary>
/// httpOnly cookies for access/refresh tokens. SameSite=None (which requires Secure=true) is used
/// whenever cookies are Secure, because the frontend and API may be deployed to different sites
/// (e.g. separate subdomains) - SameSite=Lax cookies are silently dropped from cross-site
/// fetch/XHR calls (only sent on top-level navigations), which would otherwise make every POST
/// past the first page load fail. The CSRF token is deliberately NOT relied upon being read back
/// from its cookie by the frontend either - it's returned in the JSON body of
/// login/refresh/me/external-login instead, because JS on the frontend's origin can never read a
/// cookie set for the API's origin when they're different sites.
/// </summary>
public class AuthCookieService(IConfiguration configuration)
{
    private bool Secure => configuration.GetValue<bool?>("Cookie:Secure") ?? false;
    private string SameSite => Secure ? "None" : "Lax";

    public void SetAuthCookies(HttpResponse response, IssuedTokens tokens)
    {
        var sameSite = Secure ? SameSiteMode.None : SameSiteMode.Lax;

        response.Cookies.Append("access_token", tokens.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Secure,
            SameSite = sameSite,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddSeconds(tokens.AccessTokenExpiresInSeconds),
        });

        response.Cookies.Append("refresh_token", tokens.RefreshTokenRaw, new CookieOptions
        {
            HttpOnly = true,
            Secure = Secure,
            SameSite = sameSite,
            Path = "/",
            Expires = tokens.RefreshTokenExpiresAt,
        });

        response.Cookies.Append("csrf_token", tokens.CsrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = Secure,
            SameSite = sameSite,
            Path = "/",
            Expires = tokens.RefreshTokenExpiresAt,
        });
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
        response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/" });
        response.Cookies.Delete("csrf_token", new CookieOptions { Path = "/" });
    }
}
