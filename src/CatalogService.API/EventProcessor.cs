using CatalogService.Application.Abstractions;

internal sealed class EventProcessor(IServiceProvider services, ILogger<EventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IEventPublisherService>();
                await processor.PublishPendingAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}