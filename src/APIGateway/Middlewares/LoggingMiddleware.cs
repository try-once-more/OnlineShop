using System.Diagnostics;
using Serilog.Context;

namespace APIGateway.Middlewares;

internal class LoggingMiddleware : IMiddleware
{
    private const string Header = "X-Correlation-Id";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(Header, out var values) && !string.IsNullOrWhiteSpace(values.FirstOrDefault())
                ? values.FirstOrDefault()
                : $"{Guid.NewGuid():N}";

        context.Request.Headers[Header] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[Header] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetBaggage("CorrelationId", correlationId);
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            return next(context);
        }
    }
}
