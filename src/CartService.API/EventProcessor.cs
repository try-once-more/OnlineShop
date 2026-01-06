using CartService.Application.Abstractions;

namespace CartService.API;

internal sealed class EventProcessor(IEventProcessingService eventProcessingService, ILogger<EventProcessor> logger) : BackgroundService
{
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(_delay, stoppingToken);

        await eventProcessingService.StartListeningAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Starting event processing");
                await eventProcessingService.ProcessPendingEventsAsync(100, stoppingToken);
                logger.LogDebug("Completed event processing");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            await Task.Delay(_delay, stoppingToken);
        }
    }
}
