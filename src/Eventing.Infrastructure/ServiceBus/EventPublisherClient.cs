using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Microsoft.Extensions.Logging;

namespace Eventing.Infrastructure.ServiceBus;

internal sealed class EventPublisherClient(ServiceBusClient client, string topicName, IEventConverter eventConverter, ILogger<EventPublisherClient>? logger = default)
    : IEventPublisherClient, IAsyncDisposable
{
    private readonly Lazy<ServiceBusSender> sender = new(() => client.CreateSender(topicName), LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        logger?.LogDebug("Publishing event {EventType} with MessageId={MessageId}", @event.EventType, @event.MessageId);
        try
        {
            var payload = eventConverter.Serialize(@event);
            var message = new ServiceBusMessage(payload)
            {
                MessageId = @event.MessageId.ToString(),
            };
            await sender.Value.SendMessageAsync(message, cancellationToken);
            logger?.LogDebug("Published event {EventType} with MessageId={MessageId}.", @event.EventType, @event.MessageId);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to publish event {EventType} with MessageId={MessageId}", @event.EventType, @event.MessageId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (sender.IsValueCreated)
        {
            await sender.Value.DisposeAsync();
        }
    }
}