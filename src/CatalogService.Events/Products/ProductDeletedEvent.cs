using Eventing.Abstraction;

namespace CatalogService.Events.Products;

[EventType(EventName)]
public sealed record ProductDeletedEvent : BaseEvent
{
    private const string EventName = "catalogservice.product.deleted";

    public required int Id { get; init; }

    public ProductDeletedEvent() : base(EventName) { }
}
