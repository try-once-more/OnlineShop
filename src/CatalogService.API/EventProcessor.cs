using CatalogService.Application.Abstractions;

internal sealed class EventProcessor(IServiceScopeFactory scopeFactory, ILogger<EventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
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