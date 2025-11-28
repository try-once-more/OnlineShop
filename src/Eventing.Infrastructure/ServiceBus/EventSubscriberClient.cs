using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.DependencyInjection.EventingOptions;

namespace Eventing.Infrastructure.ServiceBus;

internal sealed class EventSubscriberClient : IEventSubscriberClient, IAsyncDisposable
{
    private readonly Lazy<ServiceBusProcessor> processor;
    private readonly IEventConverter eventConverter;
    private readonly EventDispatcher dispatcher;
    private readonly ILogger<IEventSubscriberClient>? logger;

    public EventSubscriberClient(
        ServiceBusClient client,
        string topicName,
        string subscriptionName,
        EventingProcessorOptions processorOptions,
        IEventConverter eventConverter,
        EventDispatcher dispatcher,
        ILogger<IEventSubscriberClient>? logger = default)
    {
        this.eventConverter = eventConverter;
        this.dispatcher = dispatcher;
        this.logger = logger;

        processor = new Lazy<ServiceBusProcessor>(() =>
        {
            var processor = client.CreateProcessor(
                topicName,
                subscriptionName,
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = processorOptions.MaxConcurrentCalls,
                    MaxAutoLockRenewalDuration = processorOptions.MaxAutoLockRenewalDuration,
                    PrefetchCount = processorOptions.PrefetchCount,
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
        => dispatcher.Register(handler);

    public void UnregisterHandler<T>() where T : BaseEvent
        => dispatcher.Unregister<T>();

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        logger?.LogDebug("Received event with MessageId={MessageId}", messageId);
        try
        {
            BaseEvent? @event = eventConverter.Deserialize(args.Message.Body.ToStream());
            if (@event is null)
            {
                logger?.LogWarning("Unable to deserialize event with MessageId={MessageId}. Dead-lettering.", messageId);
                await args.DeadLetterMessageAsync(args.Message, "Unable to deserialize event.");
                return;
            }

            logger?.LogDebug("Dispatching event {EventType} with MessageId={MessageId}", @event.EventType, messageId);
            await dispatcher.DispatchAsync(@event, args.CancellationToken);

            logger?.LogDebug("Completed processing event with MessageId={MessageId}", messageId);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error processing event with MessageId={MessageId}", messageId);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken = default)
        => processor.Value.StartProcessingAsync(cancellationToken);

    public Task StopProcessingAsync(CancellationToken cancellationToken = default)
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
}