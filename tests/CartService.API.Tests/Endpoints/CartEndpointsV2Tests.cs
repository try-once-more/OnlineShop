using CartService.API.CartEndpoints.Contracts;
using CartService.Application.Entities;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace CartService.API.Tests.Endpoints;

[Collection(nameof(CartApiFactory))]
public class CartEndpointsV2Tests : IAsyncLifetime
{
    private readonly CartApiFactory Factory = new();
    private HttpClient client;

    public Task InitializeAsync()
    {
        client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    [Fact]
    public async Task GetCartInfoV2_ShouldReturnEmptyArray_WhenCartDoesNotExist()
    {
        var cartId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v2/cart/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<CartItemResponse[]>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetCartInfoV2_ShouldReturnItemsArray_WhenCartHasItems()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem { Id = 1, Name = "Product 1", Price = 10.00m, Quantity = 1 });
        existingCart.AddItem(new CartItem { Id = 2, Name = "Product 2", Price = 20.00m, Quantity = 2 });
        existingCart.AddItem(new CartItem { Id = 3, Name = "Product 3", Price = 30.00m, Quantity = 3 });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId))
            .ReturnsAsync(existingCart);

        var response = await client.GetAsync($"/api/v2/cart/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<CartItemResponse[]>();
        Assert.NotNull(items);
        Assert.Collection(items,
            item =>
            {
                Assert.Equal(1, item.Id);
                Assert.Equal("Product 1", item.Name);
                Assert.Equal(10.00m, item.Price);
                Assert.Equal(1, item.Quantity);
            },
            item =>
            {
                Assert.Equal(2, item.Id);
                Assert.Equal("Product 2", item.Name);
                Assert.Equal(20.00m, item.Price);
                Assert.Equal(2, item.Quantity);
            },
            item =>
            {
                Assert.Equal(3, item.Id);
                Assert.Equal("Product 3", item.Name);
                Assert.Equal(30.00m, item.Price);
                Assert.Equal(3, item.Quantity);
            });
    }

    [Fact]
    public async Task GetCartInfoV2_ShouldReturnItemsWithImages_WhenAvailable()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Product with Image",
            Price = 100.00m,
            Quantity = 1,
            Image = new ImageInfo(
                Url: new Uri("https://example.com/image1.jpg"),
                AltText: "Image 1"
            )
        });
        existingCart.AddItem(new CartItem
        {
            Id = 2,
            Name = "Product without Image",
            Price = 200.00m,
            Quantity = 2
        });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId))
            .ReturnsAsync(existingCart);

        var response = await client.GetAsync($"/api/v2/cart/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<CartItemResponse[]>();
        Assert.NotNull(items);
        Assert.Collection(items,
            item =>
            {
                Assert.Equal(1, item.Id);
                Assert.Equal("Product with Image", item.Name);
                Assert.Equal(100.00m, item.Price);
                Assert.Equal(1, item.Quantity);
                Assert.NotNull(item.Image);
                Assert.Equal("https://example.com/image1.jpg", item.Image.Url);
                Assert.Equal("Image 1", item.Image.AltText);
            },
            item =>
            {
                Assert.Equal(2, item.Id);
                Assert.Equal("Product without Image", item.Name);
                Assert.Equal(200.00m, item.Price);
                Assert.Equal(2, item.Quantity);
                Assert.Null(item.Image);
            });
    }
}
