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
        Assert.Equal(3, items.Length);

        var itemsToVerify = new[]
        {
            new { Id = 1, Name = "Product 1", Price = 10.00m, Quantity = 1 },
            new { Id = 2, Name = "Product 2", Price = 20.00m, Quantity = 2 },
            new { Id = 3, Name = "Product 3", Price = 30.00m, Quantity = 3 }
        };

        foreach (var expectedItem in itemsToVerify)
        {
            var actualItem = items.FirstOrDefault(i => i.Id == expectedItem.Id);
            Assert.NotNull(actualItem);
            Assert.Equal(expectedItem.Name, actualItem.Name);
            Assert.Equal(expectedItem.Price, actualItem.Price);
            Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
        }
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
        Assert.Equal(2, items.Length);

        var itemWithImage = items.FirstOrDefault(i => i.Id == 1);
        Assert.NotNull(itemWithImage);
        Assert.NotNull(itemWithImage.Image);
        Assert.Equal("https://example.com/image1.jpg", itemWithImage.Image.Url);
        Assert.Equal("Image 1", itemWithImage.Image.AltText);

        var itemWithoutImage = items.FirstOrDefault(i => i.Id == 2);
        Assert.NotNull(itemWithoutImage);
        Assert.Null(itemWithoutImage.Image);
    }
}
