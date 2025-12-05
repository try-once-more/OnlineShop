using System.ComponentModel.DataAnnotations;

namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Request model for adding an item to a cart.
/// </summary>
/// <param name="CartId">The unique identifier of the cart.</param>
/// <param name="Item">The cart item to add.</param>
public record AddCartItemRequest(
    [param: Required] Guid CartId,
    [param: Required] CreateCartItemRequest Item
);