using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace STLMS.API.Authorization;

/// <summary>module:action (e.g. "USERS:create") arrives as a JWT "perm" claim (see TokenService);
/// this checks the current principal has that exact claim value.</summary>
public class PermissionRequirement(string module, string action) : IAuthorizationRequirement
{
    public string PolicyName => $"{Module}:{Action}";
    public string Module { get; } = module;
    public string Action { get; } = action;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("perm", requirement.PolicyName))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>Lets [Authorize(Policy = "USERS:create")] work for any module:action pair without
/// pre-registering every combination up front - the policy is synthesized on first use.</summary>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.Contains(':')) return _fallback.GetPolicyAsync(policyName);

        var parts = policyName.Split(':', 2);
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(parts[0], parts[1]))
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}

/// <summary>[RequirePermission("USERS", "create")] - shorthand for [Authorize(Policy =
/// "USERS:create")].</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute(string module, string action)
    : Microsoft.AspNetCore.Authorization.AuthorizeAttribute($"{module}:{action}");
