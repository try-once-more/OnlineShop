using CartService.Application.Entities;

namespace CartService.Application.Abstractions;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId);
    Task AddItemAsync(Guid cartId, CartItem item);
    Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items);
    Task RemoveItemAsync(Guid cartId, CartItem item);
    Task RemoveItemsAsync(Guid cartId, IEnumerable<CartItem> items);
    Task ClearAsync(Guid cartId);
}