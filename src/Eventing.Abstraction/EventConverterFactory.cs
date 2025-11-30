using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eventing.Abstraction;

public sealed class EventConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeof(BaseEvent).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => EventConverter.Instance.Value;

    private class EventConverter : JsonConverter<BaseEvent>
    {
        internal static Lazy<EventConverter> Instance { get; } = new(() => new EventConverter(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly ConcurrentDictionary<string, Type> typeMap = new();

        private EventConverter()
        {
            // TODO: think about a more efficient way to populate typeMap 
            var baseEventType = typeof(BaseEvent);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = [.. e.Types.Where(t => t != null)];
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !baseEventType.IsAssignableFrom(type))
                        continue;

                    var attr = type.GetCustomAttribute<EventTypeAttribute>();
                    if (attr != null)
                    {
                        typeMap[attr.EventType] = type;
                    }
                }
            }
        }

        public override BaseEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var eventTypeName = options.PropertyNamingPolicy?.ConvertName(nameof(BaseEvent.EventType))
                ?? nameof(BaseEvent.EventType);

            using var doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty(eventTypeName, out var eventTypeElement)
                && eventTypeElement.ValueKind == JsonValueKind.String
                && typeMap.TryGetValue(eventTypeElement.GetString(), out var targetType))
            {
                return (BaseEvent?)doc.RootElement.Deserialize(targetType, options);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            typeMap.TryAdd(value.EventType, type);
            JsonSerializer.Serialize(writer, value, type, options);
        }
    }
}
