using CartService.Application.Abstractions;
using CatalogService.Events.Products;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CartService.Application.Handlers;

internal sealed class ProductUpdatedEventPersistHandler(IServiceScopeFactory scopeFactory, ILogger<ProductUpdatedEventPersistHandler>? logger = default)
    : IEventHandler<ProductUpdatedEvent>
{
    public async Task HandleAsync(ProductUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        logger?.LogDebug("Saving event {EventType} with MessageId={MessageId}", @event.EventType, @event.MessageId);
        await eventRepository.SaveEventAsync(@event, cancellationToken);
        logger?.LogDebug("Event {EventType} with MessageId={MessageId} saved", @event.EventType, @event.MessageId);
    }
}