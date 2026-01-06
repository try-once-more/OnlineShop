using CatalogService.Application.Abstractions;

namespace CatalogService.API;

internal sealed class EventProcessor(IServiceScopeFactory scopeFactory, ILogger<EventProcessor> logger) : BackgroundService
{
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(_delay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Starting event publishing");

                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IEventPublisherService>();
                await processor.PublishPendingAsync(cancellationToken: stoppingToken);

                logger.LogDebug("Completed event publishing");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            await Task.Delay(_delay, stoppingToken);
        }
    }
}
