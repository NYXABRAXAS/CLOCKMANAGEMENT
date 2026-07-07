using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Controllers.v1.Requests;
using STLMS.API.Middleware;
using STLMS.API.Services;
using STLMS.Application.Auth.Commands;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Auth.Queries;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IAppMediator mediator, AuthCookieService cookies, ICurrentUserService currentUser, ITokenService tokenService) : ControllerBase
{
    private string? RefreshTokenFromCookie => Request.Cookies["refresh_token"];
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UserAgentHeader => Request.Headers.UserAgent.ToString();

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password), ct);
        return Ok(result);
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.SendAsync(
            new LoginCommand(request.Email, request.Password, request.RememberMe, IpAddress, UserAgentHeader), ct);

        if (result.Auth is not null)
        {
            cookies.SetAuthCookies(Response, result.Auth.Tokens);
            return Ok(ToResponseBody(result.Auth));
        }

        return Ok(new { requiresTwoFactor = true, challengeToken = result.TwoFactorChallengeToken });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("login/2fa")]
    public async Task<IActionResult> VerifyTwoFactorLogin(VerifyTwoFactorLoginRequest request, CancellationToken ct)
    {
        var auth = await mediator.SendAsync(
            new VerifyTwoFactorLoginCommand(request.ChallengeToken, request.Code, request.RememberMe, IpAddress, UserAgentHeader), ct);
        cookies.SetAuthCookies(Response, auth.Tokens);
        return Ok(ToResponseBody(auth));
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(RefreshTokenFromCookie)) return Unauthorized(new { message = "No session." });

        var auth = await mediator.SendAsync(new RefreshTokenCommand(RefreshTokenFromCookie, IpAddress, UserAgentHeader), ct);
        cookies.SetAuthCookies(Response, auth.Tokens);
        return Ok(ToResponseBody(auth));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await mediator.SendAsync(new LogoutCommand(RefreshTokenFromCookie), ct);
        cookies.ClearAuthCookies(Response);
        return Ok(new { success = true });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new ForgotPasswordCommand(request.Email), ct);
        return Ok(new { message = "If an account exists for this email, a reset link has been sent.", devOnlyResetToken = result.DevOnlyResetToken });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
        return Ok(new { message = "Password has been reset. You can now log in." });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new VerifyEmailCommand(request.Token), ct);
        return Ok(new { message = "Email verified." });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequest request, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new ResendVerificationCommand(request.Email), ct);
        return Ok(new { message = "If an account exists for this email, a verification link has been sent.", devOnlyVerificationToken = result.DevOnlyVerificationToken });
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("external-login/google")]
    public async Task<IActionResult> ExternalLoginGoogle(ExternalLoginRequest request, CancellationToken ct)
    {
        var auth = await mediator.SendAsync(new ExternalLoginCommand(ExternalLoginProvider.Google, request.IdToken, IpAddress, UserAgentHeader), ct);
        cookies.SetAuthCookies(Response, auth.Tokens);
        return Ok(ToResponseBody(auth));
    }

    [SkipCsrf]
    [AllowAnonymous]
    [HttpPost("external-login/microsoft")]
    public async Task<IActionResult> ExternalLoginMicrosoft(ExternalLoginRequest request, CancellationToken ct)
    {
        var auth = await mediator.SendAsync(new ExternalLoginCommand(ExternalLoginProvider.Microsoft, request.IdToken, IpAddress, UserAgentHeader), ct);
        cookies.SetAuthCookies(Response, auth.Tokens);
        return Ok(ToResponseBody(auth));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var profile = await mediator.SendAsync(new GetCurrentUserQuery(currentUser.UserId!.Value), ct);
        return Ok(new { profile, csrfToken = Request.Cookies["csrf_token"] });
    }

    [HttpPost("2fa/setup")]
    public async Task<IActionResult> SetupTwoFactor(CancellationToken ct)
    {
        var result = await mediator.SendAsync(new SetupTwoFactorCommand(currentUser.UserId!.Value), ct);
        return Ok(result);
    }

    [HttpPost("2fa/verify-setup")]
    public async Task<IActionResult> VerifyTwoFactorSetup(VerifyTwoFactorSetupRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new VerifyTwoFactorSetupCommand(currentUser.UserId!.Value, request.Code), ct);
        return Ok(new { message = "Two-factor authentication enabled." });
    }

    [HttpPost("2fa/disable")]
    public async Task<IActionResult> DisableTwoFactor(DisableTwoFactorRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new DisableTwoFactorCommand(currentUser.UserId!.Value, request.Code), ct);
        return Ok(new { message = "Two-factor authentication disabled." });
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        var currentHash = RefreshTokenFromCookie is null ? null : tokenService.Hash(RefreshTokenFromCookie);
        var sessions = await mediator.SendAsync(new Application.Auth.Queries.GetSessionsQuery(currentUser.UserId!.Value, currentHash), ct);
        return Ok(sessions);
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken ct)
    {
        await mediator.SendAsync(new Application.Auth.Queries.RevokeSessionCommand(currentUser.UserId!.Value, sessionId), ct);
        return Ok(new { success = true });
    }

    private static object ToResponseBody(AuthResultDto auth) => new
    {
        profile = auth.User,
        csrfToken = auth.Tokens.CsrfToken,
    };
}
