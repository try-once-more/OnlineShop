namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Represents a shopping cart with its items.
/// </summary>
/// <param name="Id">The unique identifier of the cart.</param>
/// <param name="Items">The list of items contained in the cart.</param>
public record CartResponse(
    Guid Id,
    IReadOnlyCollection<CartItemResponse> Items
);
