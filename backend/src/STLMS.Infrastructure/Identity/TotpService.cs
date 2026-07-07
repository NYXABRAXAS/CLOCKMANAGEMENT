using OtpNet;
using QRCoder;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Identity;

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var bytes = KeyGeneration.GenerateRandomKey(20); // 160-bit, standard for TOTP
        return Base32Encoding.ToString(bytes);
    }

    public string GenerateQrCodePngBase64(string secret, string email, string issuer)
    {
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                  $"?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        var bytes = pngQrCode.GetGraphic(10);
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
