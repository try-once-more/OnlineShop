using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Eventing.Infrastructure.ServiceBus;

internal sealed class EventSubscriberFactory(IServiceProvider serviceProvider) : IEventSubscriberFactory, IAsyncDisposable
{
    private readonly ConcurrentDictionary<(string Topic, string Subscription), Lazy<EventSubscriberClient>> subscribers = new();

    public IEventSubscriberClient GetClient(string topic, string subscription)
    {
        ArgumentException.ThrowIfNullOrEmpty(topic);
        ArgumentException.ThrowIfNullOrEmpty(subscription);

        return subscribers.GetOrAdd((topic, subscription), key => new(() =>
        {
            var client = serviceProvider.GetRequiredService<ServiceBusClient>();
            var options = serviceProvider.GetRequiredService<IOptions<EventingOptions>>();
            var logger = serviceProvider.GetService<ILogger<EventSubscriberClient>>();

            return new EventSubscriberClient(client, key.Topic, key.Subscription, options.Value.ProcessorOptions, logger);
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var s in subscribers.Values)
            await s.Value.DisposeAsync();
    }
}
