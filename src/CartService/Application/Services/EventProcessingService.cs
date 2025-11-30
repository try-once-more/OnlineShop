using CartService.Application.Abstractions;
using CartService.Application.Handlers;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CartService.Application.Services;

internal class EventProcessingService(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<EventProcessingService>? logger = default)
    : IEventProcessingService
{
    private readonly Lazy<IEventSubscriberClient> subscriber = new(() =>
    {
        var eventSubscriberFactory = serviceProvider.GetRequiredService<IEventSubscriberFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<CartSubscriberOptions>>();
        var client = eventSubscriberFactory.GetClient(options.Value.TopicName, options.Value.SubscriptionName);
        return client;
    }, LazyThreadSafetyMode.ExecutionAndPublication);


    private bool isListening = false;

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (isListening)
        {
            return Task.CompletedTask;
        }

        if (!subscriber.IsValueCreated)
        {
            subscriber.Value.RegisterHandler(serviceProvider.GetRequiredService<ProductDeletedEventPersistHandler>());
            subscriber.Value.RegisterHandler(serviceProvider.GetRequiredService<ProductUpdatedEventPersistHandler>());
        }

        return subscriber.Value.StartListeningAsync(cancellationToken);
    }


    public Task StopListeningAsync(CancellationToken cancellationToken = default) =>
        subscriber.Value.StopListeningAsync(cancellationToken);

    public async Task ProcessPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var events = await eventRepository.GetPendingEventsAsync(batchSize, cancellationToken);

        foreach (var @event in events)
        {
            var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(@event.GetType());
            dynamic? handler = scope.ServiceProvider.GetService(handlerType);

            if (handler is null)
            {
                logger?.LogWarning("No handler registered for {ClrName}. Event {EventType} with MessageId={MessageId} cannot be processed.",
                    handlerType.Name, @event.EventType, @event.MessageId);
                continue;
            }

            await handler.HandleAsync((dynamic)@event, cancellationToken);
            await eventRepository.DeleteEventAsync(@event, cancellationToken);
        }
    }
}