using System.Diagnostics.CodeAnalysis;
using CartService.Application.Abstractions;
using CartService.Application.Entities;
using CartService.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using CartServiceBase = CartService.Grpc.Contracts.CartService.CartServiceBase;

namespace CartService.API.Grpc;

public sealed class CartGrpcService(
    ICartService cartService,
    ILogger<CartGrpcService> logger) : CartServiceBase
{
    public override async Task<CartItemsResponse> GetItems(
        [NotNull] GetItemsRequest request,
        [NotNull] ServerCallContext context)
    {
        var cartId = ParseCartId(request.CartId);

        var items = await cartService.GetItemsAsync(cartId, context.CancellationToken);

        logger.LogDebug("Retrieved {Count} items from cart {CartId}", items.Count, cartId);

        var response = new CartItemsResponse { CartId = cartId.ToString() };
        response.Items.AddRange(items.Adapt<IEnumerable<CartItemMessage>>());

        return response;
    }

    public override async Task GetItemsStream(
        [NotNull] GetItemsRequest request,
        [NotNull] IServerStreamWriter<CartItemMessage> responseStream,
        [NotNull] ServerCallContext context)
    {
        var cartId = ParseCartId(request.CartId);

        var items = await cartService.GetItemsAsync(cartId, context.CancellationToken);

        foreach (var item in items)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await responseStream.WriteAsync(item.Adapt<CartItemMessage>(), context.CancellationToken);
        }

        logger.LogDebug("Streamed {Count} items from cart {CartId}", items.Count, cartId);
    }

    public override async Task<AddItemsStreamResponse> AddItemsStream(
        [NotNull] IAsyncStreamReader<AddItemRequest> requestStream,
        [NotNull] ServerCallContext context)
    {
        var modifiedCartIds = new HashSet<Guid>();

        await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var cartId = ParseCartId(request.CartId);
            var cartItem = request.Item.Adapt<CartItem>();

            await cartService.AddItemAsync(cartId, cartItem, context.CancellationToken);
            modifiedCartIds.Add(cartId);
        }

        var response = new AddItemsStreamResponse();
        foreach (var cartId in modifiedCartIds)
        {
            var items = await cartService.GetItemsAsync(cartId, context.CancellationToken);
            var cart = new CartItemsResponse { CartId = cartId.ToString() };
            cart.Items.AddRange(items.Adapt<IEnumerable<CartItemMessage>>());
            response.Carts.Add(cart);
        }

        logger.LogDebug("AddItemsStream completed, modified {Count} cart(s)", modifiedCartIds.Count);

        return response;
    }

    public override async Task AddItemsBidirectional(
        [NotNull] IAsyncStreamReader<AddItemRequest> requestStream,
        [NotNull] IServerStreamWriter<CartItemsResponse> responseStream,
        [NotNull] ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var cartId = ParseCartId(request.CartId);
            var cartItem = request.Item.Adapt<CartItem>();

            await cartService.AddItemAsync(cartId, cartItem, context.CancellationToken);

            var items = await cartService.GetItemsAsync(cartId, context.CancellationToken);

            var response = new CartItemsResponse { CartId = cartId.ToString() };
            response.Items.AddRange(items.Adapt<IEnumerable<CartItemMessage>>());
            await responseStream.WriteAsync(response, context.CancellationToken);
        }
    }

    private static Guid ParseCartId(string cartId)
    {
        if (!Guid.TryParse(cartId, out var result))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid cart ID"));
        }

        return result;
    }
}
