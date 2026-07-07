namespace STLMS.API.Controllers.v1.Requests;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public record LoginRequest(string Email, string Password, bool RememberMe = false);
public record VerifyTwoFactorLoginRequest(string ChallengeToken, string Code, bool RememberMe = false);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record VerifyEmailRequest(string Token);
public record ResendVerificationRequest(string Email);
public record VerifyTwoFactorSetupRequest(string Code);
public record DisableTwoFactorRequest(string Code);
public record ExternalLoginRequest(string IdToken);
