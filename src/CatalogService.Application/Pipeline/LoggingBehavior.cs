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

        logger?.LogInformation("Request {RequestName} started", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next();
        }
        finally
        {
            stopwatch.Stop();
            logger?.LogInformation("Request {RequestName} completed in {ElapsedMilliseconds} ms",
                  requestName, stopwatch.ElapsedMilliseconds);
        }
    }
}
