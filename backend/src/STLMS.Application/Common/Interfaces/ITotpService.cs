namespace STLMS.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodePngBase64(string secret, string email, string issuer);
    bool VerifyCode(string secret, string code);
}
