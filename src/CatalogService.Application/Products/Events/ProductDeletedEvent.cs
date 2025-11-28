using Eventing.Abstraction;

namespace CatalogService.Application.Products.Events;

public sealed record ProductDeletedEvent : BaseEvent
{
    public const string EventName = "ProductDeleted";

    public required int Id { get; init; }

    public ProductDeletedEvent() : base(EventName) { }
}