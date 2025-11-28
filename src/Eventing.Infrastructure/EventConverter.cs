using Eventing.Abstraction;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eventing.Infrastructure;

internal sealed class EventConverter : JsonConverter<BaseEvent>, IEventConverter
{
    private readonly ConcurrentDictionary<string, Type> typeMap = new();
    private readonly ILogger<EventConverter>? logger;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public EventConverter(ILogger<EventConverter>? logger = default)
    {
        this.logger = logger;
        jsonSerializerOptions = new() { Converters = { this } };
    }

    public void Register(BaseEvent @event, bool force = false)
    {
        var eventType = @event.GetType();
        if (force)
        {
            typeMap[@event.EventType] = eventType;
        }
        else if (!typeMap.TryAdd(@event.EventType, eventType))
        {
            throw new InvalidOperationException($"Event type '{eventType}' is already registered.");
        }

        logger?.LogDebug("Registered event type '{EventType}' for '{ClrType}'", @event.EventType, eventType.Name);
    }

    public void Unregister(BaseEvent @event)
    {
        typeMap.TryRemove(@event.EventType, out _);
        logger?.LogDebug("Unregistered event type '{EventType}'", @event.EventType);
    }

    public string Serialize(BaseEvent @event)
    {
        typeMap.TryGetValue(@event.EventType, out var type);
        return JsonSerializer.Serialize(@event, type ?? @event.GetType(), jsonSerializerOptions);
    }

    public BaseEvent? Deserialize(string payload) =>
        JsonSerializer.Deserialize<BaseEvent>(payload, jsonSerializerOptions);

    public BaseEvent? Deserialize(Stream payload) =>
        JsonSerializer.Deserialize<BaseEvent>(payload, jsonSerializerOptions);

    public override BaseEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var eventTypeName = options.PropertyNamingPolicy?.ConvertName(nameof(BaseEvent.EventType))
            ?? nameof(BaseEvent.EventType);

        using var doc = JsonDocument.ParseValue(ref reader);
        if (doc.RootElement.TryGetProperty(eventTypeName, out var eventTypeElement)
            && typeMap.TryGetValue(eventTypeElement.GetString(), out var targetType))
        {
            return (BaseEvent?)doc.RootElement.Deserialize(targetType, options);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}