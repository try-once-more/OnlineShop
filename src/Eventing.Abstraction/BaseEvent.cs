namespace Eventing.Abstraction;

public abstract record BaseEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string EventType { get; }
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    protected BaseEvent(string eventType)
    {
        EventType = eventType;
    }
}