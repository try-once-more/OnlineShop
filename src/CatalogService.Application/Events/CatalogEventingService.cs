using CatalogService.Domain.Entities;
using Eventing.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CatalogService.Application.Events;

internal class CatalogEventingService(IOptions<CatalogEventingOptions> options, IEventConverter eventConverter)
{
    internal bool IsEnabled => !string.IsNullOrWhiteSpace(TopicName);

    internal string TopicName { get; } = options.Value.TopicName;

    internal Event ToEventEntity(BaseEvent @event) => new()
    {
        Id = @event.MessageId,
        EventType = @event.EventType,
        Payload = eventConverter.Serialize(@event),
        OccurredAtUtc = @event.OccurredAtUtc,
    };

    internal BaseEvent FromEventEntity(Event eventEntity) =>
        eventConverter.Deserialize(eventEntity.Payload)
        ?? throw new InvalidOperationException($"Unable to deserialize event {eventEntity.EventType}");
}