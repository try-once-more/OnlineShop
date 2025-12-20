using CartService.Application.Abstractions;

namespace CartService.API;

internal sealed class EventProcessor(IEventProcessingService eventProcessingService, ILogger<EventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventProcessingService.StartListeningAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await eventProcessingService.ProcessPendingEventsAsync(100, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
