using Azure.Messaging.ServiceBus;

namespace Eventing.Infrastructure;

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