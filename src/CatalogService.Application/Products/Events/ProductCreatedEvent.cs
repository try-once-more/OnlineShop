using Eventing.Abstraction;

namespace CatalogService.Application.Products.Events;

public sealed record ProductCreatedEvent : BaseEvent
{
    public const string EventName = "ProductCreated";

    public required int Id { get; init; }
    public required string Name { get; init; }
    public decimal Price { get; init; }
    public int Amount { get; init; }
    public int CategoryId { get; init; }
    public string? Description { get; init; }
    public Uri? ImageUrl { get; init; }

    public ProductCreatedEvent() : base(EventName) { }
}