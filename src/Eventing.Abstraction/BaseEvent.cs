using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Eventing.Abstraction;

[JsonConverter(typeof(EventConverterFactory))]
public abstract record BaseEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string EventType { get; }
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    protected BaseEvent([DisallowNull] string eventType)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventType, nameof(eventType));
        EventType = eventType;
    }
}
