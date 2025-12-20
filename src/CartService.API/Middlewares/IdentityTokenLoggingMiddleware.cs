using System.Security.Claims;

namespace CartService.API.Middlewares;

internal sealed class IdentityTokenLoggingMiddleware(ILogger<IdentityTokenLoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var userId = context.User.FindFirst("sub")?.Value ??
                         context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         "Unknown";

            var roles = context.User.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .Concat(context.User.FindAll("roles").Select(c => c.Value))
                        .ToList();

            var scopes = context.User.FindAll("scp")
                         .Select(c => c.Value)
                         .Concat(context.User.FindAll("http://schemas.microsoft.com/identity/claims/scope").Select(c => c.Value))
                         .ToList();

            var exp = context.User.FindFirst("exp")?.Value ?? "N/A";

            logger.LogInformation(
                "User:{UserId} | roles:[{Roles}] | scopes:[{Scopes}] | exp:{Exp} | path:{Path}",
                userId,
                string.Join(", ", roles),
                string.Join(", ", scopes),
                exp,
                context.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "path:{Path}",
                context.Request.Path);
        }

        await next(context);
    }
}
