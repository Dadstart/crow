namespace Dadstart.Labs.Crow.Server.Security;

internal sealed class SessionValidationFilter(IVaultSessionService sessionService, ILogger<SessionValidationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(VaultApiRoutes.SessionHeader, out var tokenValues))
        {
            return Results.Unauthorized();
        }

        var token = tokenValues.ToString();
        var session = await sessionService.GetAsync(token, context.HttpContext.RequestAborted);
        if (session is null)
        {
            logger.LogWarning("Rejected request with invalid session token.");
            return Results.Unauthorized();
        }

        context.HttpContext.Items[HttpContextItemKeys.Session] = session;
        return await next(context);
    }
}

