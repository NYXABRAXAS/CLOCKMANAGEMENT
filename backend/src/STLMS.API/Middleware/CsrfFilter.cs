using Microsoft.AspNetCore.Mvc.Filters;

namespace STLMS.API.Middleware;

/// <summary>Marks an action as exempt from CSRF checking - only for endpoints a client calls
/// before it has a CSRF token yet (login, register, refresh, external login).</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipCsrfAttribute : Attribute;

/// <summary>Double-submit-cookie CSRF protection. The csrf_token cookie (readable by JS on the
/// API's own origin) must be echoed back as the x-csrf-token header on any state-changing
/// request - a cross-site script/form can make the browser send the cookie automatically, but
/// can't read it to populate the header.</summary>
public class CsrfFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS" };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var skip = context.ActionDescriptor.EndpointMetadata.Any(m => m is SkipCsrfAttribute);
        var method = context.HttpContext.Request.Method;

        if (skip || SafeMethods.Contains(method))
        {
            await next();
            return;
        }

        var cookieToken = context.HttpContext.Request.Cookies["csrf_token"];
        var headerToken = context.HttpContext.Request.Headers["x-csrf-token"].ToString();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || cookieToken != headerToken)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new { message = "Invalid or missing CSRF token." })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }

        await next();
    }
}
