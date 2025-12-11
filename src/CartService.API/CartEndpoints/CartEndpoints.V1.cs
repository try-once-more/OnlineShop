using CartService.API.CartEndpoints.Contracts;
using CartService.API.Configuration;
using CartService.Application.Abstractions;
using CartService.Application.Entities;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CartService.API.Endpoints;

internal static partial class CartEndpoints
{
    extension(WebApplication app)
    {
        internal WebApplication MapCartEndpointsV1()
        {
            var group = app.MapGroup("/api/v1/cart")
            .WithTags("Cart")
            .WithGroupName("v1")
            .RequireAuthorization(nameof(PermissionOptions.ReadRole));

            group.MapGet("/{cartId:guid}", GetCartInfo)
                 .WithName(nameof(GetCartInfo));

            group.MapPost("/{cartId:guid}/items", AddCartItem)
                .WithName(nameof(AddCartItem))
                .RequireAuthorization(nameof(PermissionOptions.UpdateRole));

            group.MapDelete("/{cartId:guid}/items/{itemId:int}", DeleteCartItem)
                .WithName(nameof(DeleteCartItem))
                .RequireAuthorization(nameof(PermissionOptions.DeleteRole));

            return app;
        }
    }


    /// <summary>
    /// Retrieves a cart by its unique identifier.
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="cartService">The cart service used to fetch cart items.</param>
    /// <returns>A cart object containing the cart ID and its items.</returns>
    /// <response code="200">Successfully retrieved cart.</response>
    public static async Task<Ok<CartResponse>> GetCartInfo(Guid cartId, ICartService cartService)
    {
        var cartItems = await cartService.GetItemsAsync(cartId);
        return TypedResults.Ok(new CartResponse(cartId, cartItems.Adapt<CartItemResponse[]>()));
    }

    /// <summary>
    /// Adds an item to a cart. If the cart does not exist, it will be created.
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="request">The cart item to add to the cart.</param>
    /// <param name="cartService">The cart service used to modify cart contents.</param>
    /// <returns>200 OK if the item was added successfully.</returns>
    /// <response code="200">The item was added successfully.</response>
    public static async Task<Ok> AddCartItem(Guid cartId, CreateCartItemRequest request, ICartService cartService)
    {
        var item = request.Adapt<CartItem>();
        await cartService.AddItemAsync(cartId, item);
        return TypedResults.Ok();
    }

    /// <summary>
    /// Deletes an item from a cart.
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="itemId">The ID of the item to remove from the cart.</param>
    /// <param name="cartService">The cart service used to modify cart contents.</param>
    /// <returns>204 No Content if the item was successfully removed.</returns>
    /// <response code="204">The item was successfully removed.</response>
    public static async Task<NoContent> DeleteCartItem(Guid cartId, int itemId, ICartService cartService)
    {
        await cartService.RemoveItemAsync(cartId, itemId);
        return TypedResults.NoContent();
    }
}