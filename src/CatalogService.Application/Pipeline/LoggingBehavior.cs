using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Pipeline;

internal class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>>? logger = default)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (logger is null)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        logger?.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            logger?.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms",
                  requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger?.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
