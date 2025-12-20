using CartService.Application.Abstractions;
using Eventing.Abstraction;
using LiteDB;

namespace CartService.Infrastructure;

internal class LiteEventRepository(ILiteDatabase database) : IEventRepository
{
    private const string Collection = "events";

    public Task SaveEventAsync(BaseEvent @event, CancellationToken cancellationToken = default)
    {
        var col = database.GetCollection<EventWrapper>(Collection);
        var item = new EventWrapper
        {
            Id = @event.MessageId,
            EventType = @event.EventType,
            Payload = System.Text.Json.JsonSerializer.Serialize<BaseEvent>(@event)
        };

        col.Insert(item);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<BaseEvent>> GetPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var col = database.GetCollection<EventWrapper>(Collection);
        var wrappers = col
            .Find(Query.All("Timestamp", Query.Ascending), limit: batchSize)
            .ToList();

        var events = new List<BaseEvent>(wrappers.Count);
        foreach (var w in wrappers)
        {
            var @event = System.Text.Json.JsonSerializer.Deserialize<BaseEvent>(w.Payload);
            events.Add(@event);
        }

        return Task.FromResult((IReadOnlyList<BaseEvent>)events);
    }

    public Task DeleteEventAsync(BaseEvent @event, CancellationToken cancellationToken = default)
    {
        var col = database.GetCollection<EventWrapper>(Collection);
        col.Delete(@event.MessageId);
        return Task.CompletedTask;
    }

    private class EventWrapper
    {
        [BsonId]
        public required Guid Id { get; init; }
        public required string EventType { get; init; }
        public required string Payload { get; init; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
