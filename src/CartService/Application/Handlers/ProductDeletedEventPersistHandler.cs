using CartService.Application.Abstractions;
using CatalogService.Events.Products;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CartService.Application.Handlers;

internal sealed class ProductDeletedEventPersistHandler(IServiceScopeFactory scopeFactory, ILogger<ProductDeletedEventPersistHandler>? logger = default)
    : IEventHandler<ProductDeletedEvent>
{
    public async Task HandleAsync(ProductDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        logger?.LogDebug("Saving event {EventType} with MessageId={MessageId}", @event.EventType, @event.MessageId);
        await eventRepository.SaveEventAsync(@event, cancellationToken);
        logger?.LogDebug("Event {EventType} with MessageId={MessageId} saved", @event.EventType, @event.MessageId);
    }
}
