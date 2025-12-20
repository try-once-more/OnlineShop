using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eventing.Infrastructure.ServiceBus;

internal sealed class EventPublisherFactory(IServiceProvider serviceProvider) : IEventPublisherFactory, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<EventPublisherClient>> publishers = new();

    public IEventPublisherClient GetClient(string topic)
    {
        ArgumentException.ThrowIfNullOrEmpty(topic);

        return publishers.GetOrAdd(topic, key => new(() =>
        {
            var client = serviceProvider.GetRequiredService<ServiceBusClient>();
            var logger = serviceProvider.GetService<ILogger<EventPublisherClient>>();

            return new EventPublisherClient(client, key, logger);
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }


    public async ValueTask DisposeAsync()
    {
        foreach (var p in publishers.Values)
            await p.Value.DisposeAsync();
    }
}
