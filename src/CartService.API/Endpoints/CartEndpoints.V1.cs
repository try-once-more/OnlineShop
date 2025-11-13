using CartService.Application.Abstractions;
using CartService.Application.Entities;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CartService.API.Endpoints;

internal static partial class CartEndpoints
{
    internal static WebApplication MapCartEndpointsV1(this WebApplication app)
    {
        var group = app.MapGroup($"/api/{ApiVersions.V1}/cart")
            .WithTags("Cart")
            .WithGroupName(ApiVersions.V1);

        group.MapGet("/{cartId:guid}", GetCartInfo)
             .WithName(nameof(GetCartInfo));

        group.MapPost("/{cartId:guid}/items", AddCartItem)
            .WithName(nameof(AddCartItem));

        group.MapDelete("/{cartId:guid}/items/{itemId:int}", DeleteCartItem)
            .WithName(nameof(DeleteCartItem));

        return app;
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
        return TypedResults.Ok(new CartResponse
        {
            Id = cartId,
            Items = cartItems.Adapt<CartItemResponse[]>()
        });
    }

    /// <summary>
    /// Adds an item to a cart. If the cart does not exist, it will be created.
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="item">The <see cref="CartItem"/> to add to the cart.
    /// <para>Properties:</para>
    /// <list type="bullet">
    /// <item><c>Name</c>: Required, non-empty string.</item>
    /// <item><c>Price</c>: Required, must be greater than 0.</item>
    /// <item><c>Quantity</c>: Optional, default 1, must be greater than 0.</item>
    /// <item><c>Image</c>: Optional <see cref="ImageInfo"/> object with <c>Url</c> and <c>AltText</c>.</item>
    /// </list>
    /// </param>
    /// <param name="cartService">The cart service used to modify cart contents.</param>
    /// <returns>200 OK if the item was added successfully.</returns>
    /// <response code="200">The item was added successfully.</response>
    public static async Task<Ok> AddCartItem(Guid cartId, CartItem item, ICartService cartService)
    {
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

/// <summary>
/// Represents a shopping cart with its items.
/// </summary>
public class CartResponse
{
    /// <summary>
    /// The unique identifier of the cart.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The list of items contained in the cart.
    /// </summary>
    public IReadOnlyCollection<CartItemResponse> Items { get; set; }
}

/// <summary>
/// Represents an item within a shopping cart.
/// </summary>
public class CartItemResponse
{
    /// <summary>
    /// The unique identifier of the item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The price of the item.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The quantity of the item in the cart.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional image information associated with the item.
    /// </summary>
    public ImageInfoResponse? Image { get; set; }
}

/// <summary>
/// Represents image information for a cart item.
/// </summary>
public class ImageInfoResponse
{
    /// <summary>
    /// The URL of the image.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Alternative text describing the image.
    /// </summary>
    public string AltText { get; set; }
}