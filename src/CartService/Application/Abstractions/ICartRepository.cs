using CartService.Application.Entities;

namespace CartService.Application.Abstractions;

public interface ICartRepository
{
    Task<Cart?> GetAsync(Guid cartId);
    Task SaveAsync(Cart cart);
}
