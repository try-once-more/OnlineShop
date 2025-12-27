using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Eventing.Abstraction;
using Eventing.Infrastructure;
using Eventing.Infrastructure.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

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

                if (!string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
                {
                    return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential(), clientOptions);
                }
                if (!string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    return new ServiceBusClient(options.ConnectionString, clientOptions);
                }
                throw new InvalidOperationException("Either FullyQualifiedNamespace or ConnectionString must be configured.");
            });
        });

        services.AddSingleton<IEventPublisherFactory, EventPublisherFactory>();
        services.AddSingleton<IEventSubscriberFactory, EventSubscriberFactory>();

        return services;
    }
}
