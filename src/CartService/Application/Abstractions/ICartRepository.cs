using CartService.Application.Entities;

namespace CartService.Application.Abstractions;

public interface ICartRepository
{
    Task<Cart?> GetAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Cart>> GetCartsByItemIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task SaveAsync(Cart cart, CancellationToken cancellationToken = default);
    Task SaveAsync(IEnumerable<Cart> carts, CancellationToken cancellationToken = default);
}
