using STLMS.Infrastructure.Identity;
using Xunit;

namespace STLMS.Application.Tests.Identity;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesADifferentStringThanTheInput()
    {
        var hash = _sut.Hash("Sup3r$ecret");
        Assert.NotEqual("Sup3r$ecret", hash);
        Assert.StartsWith("$2", hash); // BCrypt format marker
    }

    [Fact]
    public void Hash_IsSaltedSoTheSamePasswordProducesDifferentHashes()
    {
        var hash1 = _sut.Hash("Sup3r$ecret");
        var hash2 = _sut.Hash("Sup3r$ecret");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ReturnsTrueForTheCorrectPassword()
    {
        var hash = _sut.Hash("Sup3r$ecret");
        Assert.True(_sut.Verify("Sup3r$ecret", hash));
    }

    [Fact]
    public void Verify_ReturnsFalseForTheWrongPassword()
    {
        var hash = _sut.Hash("Sup3r$ecret");
        Assert.False(_sut.Verify("wrong-password", hash));
    }
}
