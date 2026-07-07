using STLMS.Infrastructure.Identity;
using Xunit;

namespace STLMS.Application.Tests.Identity;

public class TotpServiceTests
{
    private readonly TotpService _sut = new();

    [Fact]
    public void GenerateSecret_ProducesANonEmptyBase32String()
    {
        var secret = _sut.GenerateSecret();
        Assert.False(string.IsNullOrWhiteSpace(secret));
        Assert.All(secret, c => Assert.Contains(c, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"));
    }

    [Fact]
    public void VerifyCode_AcceptsTheCurrentCodeForASecret()
    {
        var secret = _sut.GenerateSecret();
        var currentCode = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(secret)).ComputeTotp();

        Assert.True(_sut.VerifyCode(secret, currentCode));
    }

    [Fact]
    public void VerifyCode_RejectsAWrongCode()
    {
        var secret = _sut.GenerateSecret();
        Assert.False(_sut.VerifyCode(secret, "000000"));
    }

    [Fact]
    public void GenerateQrCodePngBase64_ProducesDecodableBase64Png()
    {
        var secret = _sut.GenerateSecret();
        var qr = _sut.GenerateQrCodePngBase64(secret, "user@example.com", "STLMS");

        var bytes = Convert.FromBase64String(qr);
        // PNG file signature
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'N', bytes[2]);
        Assert.Equal((byte)'G', bytes[3]);
    }
}
