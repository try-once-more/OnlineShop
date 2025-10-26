using CartService.Application.Abstractions;
using CartService.Application.Entities;

namespace CartService.Application.Services;

public class CartAppService(ICartRepository cartRepository) : ICartService
{
    private readonly ICartRepository cartRepository = cartRepository;

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId)
    {
        var cart = await cartRepository.GetAsync(cartId);
        return cart?.Items ?? [];
    }

    public async Task AddItemAsync(Guid cartId, CartItem item)
    {
        var cart = await cartRepository.GetAsync(cartId) ?? new Cart { Id = cartId };
        cart.AddItem(item);
        await cartRepository.SaveAsync(cart);
    }

    public async Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items)
    {
        var cart = await cartRepository.GetAsync(cartId) ?? new Cart { Id = cartId };
        foreach (var item in items)
        {
            cart.AddItem(item);
        }
        await cartRepository.SaveAsync(cart);
    }

    public async Task RemoveItemAsync(Guid cartId, CartItem item)
    {
        var cart = await cartRepository.GetAsync(cartId);
        if (cart is not null)
        {
            var hasChanges = cart.RemoveItem(item);
            if (hasChanges)
            {
                await cartRepository.SaveAsync(cart);
            }
        }
    }

    public async Task RemoveItemsAsync(Guid cartId, IEnumerable<CartItem> items)
    {
        var cart = await cartRepository.GetAsync(cartId);
        if (cart is not null)
        {
            bool hasChanges = false;
            foreach (var item in items)
            {
                hasChanges |= cart.RemoveItem(item);
            }

            if (hasChanges)
            {
                await cartRepository.SaveAsync(cart);
            }
        }
    }

    public async Task ClearAsync(Guid cartId)
    {
        var cart = await cartRepository.GetAsync(cartId);
        if (cart is not null)
        {
            var hasChanges = cart.Clear();
            if (hasChanges)
            {
                await cartRepository.SaveAsync(cart);
            }
        }
    }
}
