using CartService.Application.Entities;

namespace CartService.Application.Abstractions;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId);
    Task AddItemAsync(Guid cartId, CartItem item);
    Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items);
    Task ChangeItemQuantityAsync(Guid cartId, int itemId, int newQuantity);
    Task RemoveItemAsync(Guid cartId, int itemId);
    Task RemoveItemsAsync(Guid cartId, IEnumerable<int> itemIds);
    Task ClearAsync(Guid cartId);
}