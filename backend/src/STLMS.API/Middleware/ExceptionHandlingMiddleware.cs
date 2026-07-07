using System.Net;
using System.Text.Json;
using STLMS.Application.Common.Exceptions;
using ValidationException = STLMS.Application.Common.Exceptions.ValidationException;

namespace STLMS.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationEx => (HttpStatusCode.BadRequest, "Validation failed.", (object?)validationEx.Errors),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            ConflictException => (HttpStatusCode.Conflict, exception.Message, null),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message, null),
            UnauthorizedAppException => (HttpStatusCode.Unauthorized, exception.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("{StatusCode} handling {Method} {Path}: {Message}", (int)statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new { statusCode = (int)statusCode, title, errors, traceId = context.TraceIdentifier };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
