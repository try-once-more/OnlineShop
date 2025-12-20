using System.Diagnostics.CodeAnalysis;

namespace Eventing.Abstraction;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EventTypeAttribute : Attribute
{
    public string EventType { get; }

    public EventTypeAttribute([DisallowNull] string eventType)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventType, nameof(eventType));
        EventType = eventType;
    }
}
