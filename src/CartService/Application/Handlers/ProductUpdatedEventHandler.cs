using CartService.Application.Abstractions;
using CartService.Application.Entities;
using CatalogService.Events.Products;

namespace CartService.Application.Handlers;

internal sealed class ProductUpdatedEventHandler(ICartService cartService) : IIntegrationEventHandler<ProductUpdatedEvent>
{
    public Task HandleAsync(ProductUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        var item = new CartItem
        {
            Id = @event.Id,
            Name = @event.Name,
            Price = @event.Price,
            Image = @event.ImageUrl is null ? null : new ImageInfo(@event.ImageUrl)
        };

        return cartService.UpdateItemInCartsAsync(item, cancellationToken);
    }
}
