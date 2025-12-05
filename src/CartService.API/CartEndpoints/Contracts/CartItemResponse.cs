using CartService.Application.Entities;

namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Represents an item within a shopping cart.
/// </summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="Name">The name of the item.</param>
/// <param name="Price">The price of the item.</param>
/// <param name="Quantity">The quantity of the item in the cart.</param>
/// <param name="Image">Optional image information associated with the item.</param>
/// <param name="Status">The current status of the item.</param>
public record CartItemResponse(
    int Id,
    string Name,
    decimal Price,
    int Quantity,
    ImageInfoResponse? Image = null,
    CartItemStatus Status = CartItemStatus.Available
);