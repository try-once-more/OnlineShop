using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using static Eventing.Infrastructure.EventingOptions;

namespace Eventing.Infrastructure.ServiceBus;

internal sealed class EventSubscriberClient : IEventSubscriberClient, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Type, IEventHandlerInvoker> handlers = [];
    private readonly Lazy<ServiceBusProcessor> processor;
    private readonly ILogger<IEventSubscriberClient>? logger;

    public EventSubscriberClient(ServiceBusClient client, string topicName, string subscriptionName, EventingProcessorOptions options,
        ILogger<IEventSubscriberClient>? logger = default)
    {
        this.logger = logger;

        processor = new Lazy<ServiceBusProcessor>(() =>
        {
            var processor = client.CreateProcessor(
                topicName,
                subscriptionName,
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = options.MaxConcurrentCalls,
                    MaxAutoLockRenewalDuration = options.MaxAutoLockRenewalDuration,
                    PrefetchCount = options.PrefetchCount,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                    AutoCompleteMessages = false
                });

            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += args =>
            {
                logger?.LogError(args.Exception, "Error processing events.");
                return Task.CompletedTask;
            };

            return processor;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public void RegisterHandler<T>(IEventHandler<T> handler) where T : BaseEvent
    {
        var type = typeof(T);
        if (!handlers.TryAdd(type, new EventHandlerInvoker<T>(handler)))
        {
            logger?.LogError("Handler for {Type} already registered.", type.Name);
            throw new InvalidOperationException($"Handler for {type.Name} already registered.");
        }

        logger?.LogInformation("Registered handler for {Type}", type.Name);
    }

    public void UnregisterHandler<T>() where T : BaseEvent
    {
        var type = typeof(T);
        handlers.TryRemove(type, out _);
        logger?.LogInformation("Unregistered handler for {Type}", type.Name);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        logger?.LogDebug("Received event with MessageId={MessageId}", messageId);
        try
        {
            BaseEvent? @event = JsonSerializer.Deserialize<BaseEvent>(args.Message.Body);
            if (@event is null)
            {
                logger?.LogWarning("Unable to deserialize event with MessageId={MessageId}. Dead-lettering.", messageId);
                await args.DeadLetterMessageAsync(args.Message, "Unable to deserialize event.");
                return;
            }

            logger?.LogDebug("Processing event {EventType} with MessageId={MessageId}", @event.EventType, messageId);

            var type = @event.GetType();
            if (handlers.TryGetValue(type, out var invoker))
            {
                await invoker.InvokeAsync(@event, args.CancellationToken);
                logger?.LogDebug("Processing event {EventType} with MessageId={MessageId}", @event.EventType, messageId);
            }
            else
            {
                logger?.LogWarning("Unable to process event {EventType} with MessageId={MessageId}. No handler registered for {Type}",
                    @event.EventType, messageId, type.Name);
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error processing event with MessageId={MessageId}", messageId);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
        => processor.Value.StartProcessingAsync(cancellationToken);

    public Task StopListeningAsync(CancellationToken cancellationToken = default)
        => processor.Value.StopProcessingAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (processor.IsValueCreated)
        {
            await processor.Value.StopProcessingAsync();
            processor.Value.ProcessMessageAsync -= ProcessMessageAsync;
            await processor.Value.DisposeAsync();
        }
    }

    private interface IEventHandlerInvoker
    {
        Task InvokeAsync(BaseEvent @event, CancellationToken cancellationToken);
    }

    private class EventHandlerInvoker<T>(IEventHandler<T> handler) : IEventHandlerInvoker where T : BaseEvent
    {
        public Task InvokeAsync(BaseEvent @event, CancellationToken cancellationToken)
            => handler.HandleAsync((T)@event, cancellationToken);
    }
}