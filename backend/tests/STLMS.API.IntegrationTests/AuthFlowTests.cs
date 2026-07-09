using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace STLMS.API.IntegrationTests;

/// <summary>Drives the real HTTP pipeline (real middleware, real JWT/cookie auth, real DbSeeder-
/// seeded roles/permissions) end-to-end against a throwaway SQLite database - the one thing unit
/// tests with fakes/mocks can never prove: that registration, email verification, login, and a
/// permission-gated endpoint actually work wired together the way a real client would call them.</summary>
public class AuthFlowTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static string ExtractCookieHeader(HttpResponseMessage response)
    {
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : [];
        return string.Join("; ", setCookies.Select(c => c.Split(';')[0]));
    }

    [Fact]
    public async Task RegisterVerifyLogin_ThenCallingAProtectedEndpoint_Succeeds()
    {
        var client = factory.CreateClient();
        var email = $"itest-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register", new { firstName = "Integration", lastName = "Test", email, password = "Passw0rd!123" });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var verificationToken = registerBody.GetProperty("devOnlyVerificationToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(verificationToken));

        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new { token = verificationToken });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "Passw0rd!123", rememberMe = false });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var csrfToken = loginBody.GetProperty("csrfToken").GetString();
        var cookieHeader = ExtractCookieHeader(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(csrfToken));
        Assert.False(string.IsNullOrWhiteSpace(cookieHeader));

        using var protectedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/notifications");
        protectedRequest.Headers.Add("Cookie", cookieHeader);
        var protectedResponse = await client.SendAsync(protectedRequest);

        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthentication_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = factory.CreateClient();
        var email = $"itest-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new { firstName = "Integration", lastName = "Test", email, password = "Passw0rd!123" });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "WrongPassword!1", rememberMe = false });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var email = $"itest-{Guid.NewGuid():N}@example.com";
        var payload = new { firstName = "Integration", lastName = "Test", email, password = "Passw0rd!123" };
        await client.PostAsJsonAsync("/api/v1/auth/register", payload);

        var secondResponse = await client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_IsReachableWithoutAuthentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
