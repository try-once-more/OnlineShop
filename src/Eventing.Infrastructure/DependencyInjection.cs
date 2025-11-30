using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Eventing.Infrastructure.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public record EventingOptions
{
    private static readonly ServiceBusRetryOptions DefaultRetryOptions = new();

    public required string FullyQualifiedNamespace { get; init; }
    public EventingRetryOptions RetryOptions { get; init; } = new();
    public EventingProcessorOptions ProcessorOptions { get; init; } = new();

    public sealed record EventingRetryOptions
    {
        public ServiceBusRetryMode Mode { get; init; } = DefaultRetryOptions.Mode;
        public int MaxRetries { get; init; } = DefaultRetryOptions.MaxRetries;
        public TimeSpan Delay { get; init; } = DefaultRetryOptions.Delay;
        public TimeSpan MaxDelay { get; init; } = DefaultRetryOptions.MaxDelay;
        public TimeSpan TryTimeout { get; init; } = DefaultRetryOptions.TryTimeout;
    }

    public sealed record EventingProcessorOptions
    {
        public int PrefetchCount { get; init; }
        public int MaxConcurrentCalls { get; init; } = 1;
        public TimeSpan MaxAutoLockRenewalDuration { get; init; } = TimeSpan.FromMinutes(5);
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddEventing(this IServiceCollection services)
    {
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddClient<ServiceBusClient, ServiceBusClientOptions>((_, _, sp) =>
            {
                var options = sp.GetRequiredService<IOptions<EventingOptions>>().Value;
                var clientOptions = new ServiceBusClientOptions
                {
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        Mode = options.RetryOptions.Mode,
                        MaxRetries = options.RetryOptions.MaxRetries,
                        Delay = options.RetryOptions.Delay,
                        MaxDelay = options.RetryOptions.MaxDelay,
                        TryTimeout = options.RetryOptions.TryTimeout
                    }
                };

                return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential(), clientOptions);
            });
        });

        services.AddSingleton<IEventPublisherFactory, EventPublisherFactory>();
        services.AddSingleton<IEventSubscriberFactory, EventSubscriberFactory>();

        return services;
    }
}