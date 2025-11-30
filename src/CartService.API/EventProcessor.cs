
using CartService.Application.Abstractions;

internal sealed class EventProcessor(IEventProcessingService eventProcessingService, ILogger<EventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await eventProcessingService.StartListeningAsync(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await eventProcessingService.ProcessPendingEventsAsync(100, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}