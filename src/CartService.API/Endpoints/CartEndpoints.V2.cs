using CartService.Application.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CartService.API.Endpoints
{
    internal static partial class CartEndpoints
    {
        internal static WebApplication MapCartEndpointsV2(this WebApplication app)
        {
            var group = app.MapGroup($"/api/{ApiVersions.V2}/cart")
                .WithTags("Cart")
                .WithGroupName(ApiVersions.V2);

            group.MapGet("/{cartId:guid}", GetCartInfoV2)
                .WithName(nameof(GetCartInfoV2));

            return app;
        }

        /// <summary>
        /// Retrieves a cart by its unique identifier.
        /// </summary>
        /// <param name="cartId">The unique identifier of the cart.</param>
        /// <param name="cartService">The cart service used to fetch cart items.</param>
        /// <returns>A cart object containing the cart ID and its items.</returns>
        /// <response code="200">Successfully retrieved cart.</response>
        public static async Task<Ok<CartItemResponse[]>> GetCartInfoV2(Guid cartId, ICartService cartService)
        {
            var cartItems = await cartService.GetItemsAsync(cartId);
            return TypedResults.Ok(cartItems.Adapt<CartItemResponse[]>());
        }
    }
}
