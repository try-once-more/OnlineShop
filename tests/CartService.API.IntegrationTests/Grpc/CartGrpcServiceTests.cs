using CartService.Application.Entities;
using CartService.Grpc.Contracts;
using Grpc.Core;
using Moq;
using GrpcCartService = CartService.Grpc.Contracts.CartService;

namespace CartService.API.IntegrationTests.Grpc;

[Collection(nameof(CartApiFactory))]
public class CartGrpcServiceTests : IAsyncLifetime
{
    private readonly CartApiFactory Factory = new();
    private GrpcCartService.CartServiceClient client;

    public Task InitializeAsync()
    {
        client = Factory.CreateGrpcClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    [Fact]
    public async Task GetItems_WhenCartExists_ReturnsItems()
    {
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId };
        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Product",
            Price = 99.99m,
            Quantity = 2
        });

        Factory.MockRepository
            .Setup(r => r.GetAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var response = await client.GetItemsAsync(
            new GetItemsRequest { CartId = cartId.ToString() });

        Assert.Equal(cartId.ToString(), response.CartId);
        Assert.Single(response.Items);

        var item = response.Items[0];
        Assert.Equal(1, item.Id);
        Assert.Equal("Product", item.Name);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(99.99m, (decimal)item.Price);
    }


    [Fact]
    public async Task GetItemsStream_WhenCartHasItems_StreamsAllItems()
    {
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId };
        cart.AddItem(new CartItem { Id = 1, Name = "Item1", Price = 10m, Quantity = 1 });
        cart.AddItem(new CartItem { Id = 2, Name = "Item2", Price = 20m, Quantity = 1 });

        Factory.MockRepository
            .Setup(r => r.GetAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var call = client.GetItemsStream(
            new GetItemsRequest { CartId = cartId.ToString() });

        var items = await call.ResponseStream
            .ReadAllAsync()
            .Select(i => i.Name)
            .ToListAsync();

        Assert.Equal(new[] { "Item1", "Item2" }, items);
    }


    [Fact]
    public async Task AddItemsStream_WhenItemsAdded_ReturnsAffectedCart()
    {
        var cartId = Guid.NewGuid();
        var call = client.AddItemsStream();

        await call.RequestStream.WriteAsync(new AddItemRequest
        {
            CartId = cartId.ToString(),
            Item = new CartItemMessage
            {
                Id = 1,
                Name = "Item",
                Quantity = 1,
                Price = new DecimalValue { Units = 10 }
            }
        });

        await call.RequestStream.CompleteAsync();

        var response = await call;

        Assert.Single(response.Carts);
        Assert.Equal(cartId.ToString(), response.Carts[0].CartId);
        Assert.Single(response.Carts[0].Items);
    }

    [Fact]
    public async Task AddItemsBidirectional_WhenItemAdded_StreamsUpdatedCart()
    {
        var cartId = Guid.NewGuid();
        using var call = client.AddItemsBidirectional();

        var responses = new List<CartItemsResponse>();
        var readTask = Task.Run(async () =>
        {
            await foreach (var r in call.ResponseStream.ReadAllAsync())
                responses.Add(r);
        });

        await call.RequestStream.WriteAsync(new AddItemRequest
        {
            CartId = cartId.ToString(),
            Item = new CartItemMessage
            {
                Id = 1,
                Name = "Item",
                Quantity = 1,
                Price = new DecimalValue { Units = 10 }
            }
        });

        await call.RequestStream.CompleteAsync();
        await readTask;

        Assert.Single(responses);
        Assert.Equal(cartId.ToString(), responses[0].CartId);
        Assert.Single(responses[0].Items);
    }


    [Fact]
    public async Task GetItems_WhenInvalidCartId_ReturnsInvalidArgument()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetItemsAsync(new GetItemsRequest { CartId = "invalid-guid" }));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task AddItemsStream_WhenServiceThrows_ReturnsInvalidArgument()
    {
        var cartId = Guid.NewGuid();
        var call = client.AddItemsStream();

        await call.RequestStream.WriteAsync(new AddItemRequest
        {
            CartId = cartId.ToString(),
            Item = new CartItemMessage
            {
                Id = 1,
                Name = "Item",
                Quantity = 1,
                Price = new DecimalValue { Units = 0 }
            }
        });

        await call.RequestStream.CompleteAsync();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await call);

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }
}
