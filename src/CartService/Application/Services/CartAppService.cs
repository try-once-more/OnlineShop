using CartService.Application.Abstractions;
using CartService.Application.Entities;

namespace CartService.Application.Services;

public class CartAppService(ICartRepository cartRepository) : ICartService
{
    private readonly ICartRepository cartRepository = cartRepository;

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cartId, cancellationToken);
        return cart?.Items ?? [];
    }

    public async Task AddItemAsync(Guid cartId, CartItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        var cart = await GetCartAsync(cartId, cancellationToken) ?? new Cart { Id = cartId };
        cart.AddItem(item);
        await cartRepository.SaveAsync(cart, cancellationToken);
    }

    public async Task AddItemsAsync(Guid cartId, IEnumerable<CartItem> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        var cart = await GetCartAsync(cartId, cancellationToken) ?? new Cart { Id = cartId };
        foreach (var item in items)
        {
            cart.AddItem(item);
        }
        await cartRepository.SaveAsync(cart, cancellationToken);
    }

    public async Task ChangeItemQuantityAsync(Guid cartId, int itemId, int newQuantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cartId, cancellationToken) ?? throw new CartNotFoundException(cartId);
        var item = cart.Items.FirstOrDefault(i => i.Id.Equals(itemId)) ?? throw new CartItemNotAddedException(itemId, cartId);
        item.Quantity = newQuantity;
        await cartRepository.SaveAsync(cart, cancellationToken);
    }

    public async Task RemoveItemAsync(Guid cartId, int itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cartId, cancellationToken);
        if (cart is null)
            return;

        var hasChanges = cart.RemoveItem(itemId);
        if (hasChanges)
            await cartRepository.SaveAsync(cart, cancellationToken);
    }

    public async Task RemoveItemsAsync(Guid cartId, IEnumerable<int> itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var cart = await GetCartAsync(cartId, cancellationToken);
        if (cart is null)
            return;

        bool hasChanges = false;
        foreach (var itemId in itemIds)
        {
            hasChanges |= cart.RemoveItem(itemId);
        }

        if (hasChanges)
            await cartRepository.SaveAsync(cart, cancellationToken);
    }

    public async Task ClearAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cartId, cancellationToken);
        if (cart is null)
            return;

        var hasChanges = cart.Clear();
        if (hasChanges)
            await cartRepository.SaveAsync(cart, cancellationToken);
    }

    private async Task<Cart?> GetCartAsync(Guid cartId, CancellationToken cancellationToken) =>
        await cartRepository.GetAsync(cartId, cancellationToken);

    public async Task<int> UpdateItemInCartsAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cartItem);
        var carts = await cartRepository.GetCartsByItemIdAsync(cartItem.Id, cancellationToken);
        foreach (var item in carts.SelectMany(c => c.Items.Where(i => i.Id == cartItem.Id)))
        {
            item.Name = cartItem.Name;
            item.Price = cartItem.Price;
            item.Image = cartItem.Image;
        }

        await cartRepository.SaveAsync(carts, cancellationToken);
        return carts.Count;
    }

    public async Task<int> DiscontinueItemInCartsAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var carts = await cartRepository.GetCartsByItemIdAsync(itemId, cancellationToken);
        foreach (var item in carts.SelectMany(c => c.Items.Where(i => i.Id == itemId)))
        {
            item.Status = CartItemStatus.Discontinued;
        }

        await cartRepository.SaveAsync(carts, cancellationToken);
        return carts.Count;
    }
}
