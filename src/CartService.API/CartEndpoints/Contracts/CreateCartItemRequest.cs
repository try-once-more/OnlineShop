using CartService.Application.Entities;
using System.ComponentModel.DataAnnotations;

namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Request model for creating a cart item.
/// </summary>
/// <param name="Id">The unique identifier of the item. Must be greater than 0.</param>
/// <param name="Name">The name of the item. Cannot be empty.</param>
/// <param name="Price">The price of the item. Must be greater than 0.</param>
/// <param name="Quantity">The quantity of the item. Must be greater than 0.</param>
/// <param name="Image">Optional image information associated with the item.</param>
/// <param name="Status">The current status of the item.</param>
public record CreateCartItemRequest(
    [param: Required, Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0.")] int Id,
    [param: Required, MinLength(1, ErrorMessage = "Name cannot be empty.")] string Name,
    [param: Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")] decimal Price,
    [param: Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")] int Quantity = 1,
    CreateImageInfoRequest? Image = null,
    CartItemStatus Status = CartItemStatus.Available
);