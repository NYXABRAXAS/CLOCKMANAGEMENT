using Microsoft.Extensions.Configuration;
using STLMS.Infrastructure.Identity;
using Xunit;

namespace STLMS.Application.Tests.Identity;

public class TokenServiceTests
{
    private static TokenService BuildSut() =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "unit-test-signing-secret-at-least-32-bytes-long",
            ["Jwt:Issuer"] = "STLMS.Tests",
            ["Jwt:Audience"] = "STLMS.Tests.Client",
            ["Jwt:AccessTokenMinutes"] = "5",
        }).Build());

    [Fact]
    public void GenerateAccessToken_ProducesAThreePartJwt()
    {
        var sut = BuildSut();
        var token = sut.GenerateAccessToken(Guid.NewGuid(), "a@b.com", ["STANDARD_USER"], ["DASHBOARD:view"]);
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void GenerateRefreshToken_RawAndHash_AreDifferentAndHashIsDeterministic()
    {
        var sut = BuildSut();
        var (raw, hash) = sut.GenerateRefreshToken();

        Assert.NotEqual(raw, hash);
        Assert.Equal(hash, sut.Hash(raw));
    }

    [Fact]
    public void GenerateRefreshToken_ProducesAUniqueTokenEachCall()
    {
        var sut = BuildSut();
        var (raw1, _) = sut.GenerateRefreshToken();
        var (raw2, _) = sut.GenerateRefreshToken();
        Assert.NotEqual(raw1, raw2);
    }

    [Fact]
    public void AccessTokenMinutes_ReadsFromConfiguration()
    {
        var sut = BuildSut();
        Assert.Equal(5, sut.AccessTokenMinutes);
    }
}
