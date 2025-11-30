using CartService.Application.Abstractions;
using CatalogService.Events.Products;

namespace CartService.Application.Handlers;


internal sealed class ProductDeletedEventHandler(ICartService cartService) : IIntegrationEventHandler<ProductDeletedEvent>
{
    public Task HandleAsync(ProductDeletedEvent @event, CancellationToken cancellationToken = default) =>
        cartService.DiscontinueItemInCartsAsync(@event.Id, cancellationToken);
}
