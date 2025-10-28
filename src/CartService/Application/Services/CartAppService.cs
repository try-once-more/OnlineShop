using CartService.Application.Abstractions;
using CartService.Application.Entities;

namespace CartService.Application.Services;

public class CartAppService(ICartRepository cartRepository) : ICartService
{
    private readonly ICartRepository cartRepository = cartRepository;

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId)
    {
        var cart = await GetCartAsync(cartId);
        return cart?.Items ?? [];
    }

    public async Task AddItemAsync(Guid cartId, CartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var cart = await GetCartAsync(cartId) ?? new Cart { Id = cartId };
        cart.AddItem(item);
        await cartRepository.SaveAsync(cart);
    }

    public async Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var cart = await GetCartAsync(cartId) ?? new Cart { Id = cartId };
        foreach (var item in items)
        {
            cart.AddItem(item);
        }
        await cartRepository.SaveAsync(cart);
    }

    public async Task ChangeItemQuantityAsync(Guid cartId, int itemId, int newQuantity)
    {
        var cart = await GetCartAsync(cartId) ?? throw new CartNotFoundException(cartId);
        var item = cart.Items.FirstOrDefault(i => i.Id.Equals(itemId)) ?? throw new CartItemNotAddedException(itemId, cartId);
        item.Quantity = newQuantity;
        await cartRepository.SaveAsync(cart);
    }

    public async Task RemoveItemAsync(Guid cartId, int itemId)
    {
        var cart = await GetCartAsync(cartId);
        if (cart is null)
            return;

        var hasChanges = cart.RemoveItem(itemId);
        if (hasChanges)
            await cartRepository.SaveAsync(cart);
    }

    public async Task RemoveItemsAsync(Guid cartId, IEnumerable<int> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var cart = await GetCartAsync(cartId);
        if (cart is null)
            return;

        bool hasChanges = false;
        foreach (var itemId in itemIds)
        {
            hasChanges |= cart.RemoveItem(itemId);
        }

        if (hasChanges)
            await cartRepository.SaveAsync(cart);
    }

    public async Task ClearAsync(Guid cartId)
    {
        var cart = await GetCartAsync(cartId);
        if (cart is null)
            return;

        var hasChanges = cart.Clear();
        if (hasChanges)
            await cartRepository.SaveAsync(cart);
    }

    private async Task<Cart?> GetCartAsync(Guid cartId) => await cartRepository.GetAsync(cartId);
}
