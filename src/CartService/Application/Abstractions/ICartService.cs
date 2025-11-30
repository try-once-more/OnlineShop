using CartService.Application.Entities;

namespace CartService.Application.Abstractions;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddItemAsync(Guid cartId, CartItem item, CancellationToken cancellationToken = default);
    Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items, CancellationToken cancellationToken = default);
    Task ChangeItemQuantityAsync(Guid cartId, int itemId, int newQuantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid cartId, int itemId, CancellationToken cancellationToken = default);
    Task RemoveItemsAsync(Guid cartId, IEnumerable<int> itemIds, CancellationToken cancellationToken = default);
    Task ClearAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<int> UpdateItemInCartsAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task<int> DiscontinueItemInCartsAsync(int itemId, CancellationToken cancellationToken = default);
}