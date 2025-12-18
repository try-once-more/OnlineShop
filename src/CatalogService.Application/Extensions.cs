using System.Text.Json;
using CatalogService.Domain.Entities;
using Eventing.Abstraction;

namespace CatalogService.Application;

internal static class EventExtensions
{
    extension(BaseEvent source)
    {
        internal Event ToEventEntity() =>
            new()
            {
                Id = source.MessageId,
                EventType = source.EventType,
                Payload = JsonSerializer.Serialize<BaseEvent>(source),
                OccurredAtUtc = source.OccurredAtUtc
            };
    }

    extension(Event source)
    {
        internal BaseEvent ToEvent() => JsonSerializer.Deserialize<BaseEvent>(source.Payload)
            ?? throw new InvalidOperationException($"Unable to deserialize event {source.EventType}");
    }
}
