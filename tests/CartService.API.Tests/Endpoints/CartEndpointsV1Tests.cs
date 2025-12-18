using System.Net;
using System.Net.Http.Json;
using CartService.API.CartEndpoints.Contracts;
using CartService.Application.Entities;
using Moq;

namespace CartService.API.Tests.Endpoints;

[Collection(nameof(CartApiFactory))]
public class CartEndpointsV1Tests : IAsyncLifetime
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
    public async Task GetCartInfo_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        var cartId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/cart/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(cart);
        Assert.Equal(cartId, cart.Id);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task AddCartItem_ShouldAddItemToCart()
    {
        var cartId = Guid.NewGuid();
        var item = new CartItem
        {
            Id = 1,
            Name = "Test Product",
            Price = 99.99m,
            Quantity = 2,
            Image = new ImageInfo(
                Url: new Uri("https://example.com/product.jpg"),
                AltText: "Test Product Image"
            )
        };

        var addResponse = await client.PostAsJsonAsync($"/api/v1/cart/{cartId}/items", item);

        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        Factory.MockRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cartId &&
            c.Items.Count == 1 &&
            c.Items[0].Id == item.Id &&
            c.Items[0].Name == item.Name &&
            c.Items[0].Price == item.Price &&
            c.Items[0].Quantity == item.Quantity &&
            c.Items[0].Image != null &&
            c.Items[0].Image.Url == item.Image.Url &&
            c.Items[0].Image.AltText == item.Image.AltText
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddCartItem_ShouldIncreaseQuantity_WhenItemAlreadyExists()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test Product",
            Price = 99.99m,
            Quantity = 2
        });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId))
            .ReturnsAsync(existingCart);

        var itemToAdd = new CartItem
        {
            Id = 1,
            Name = "Test Product",
            Price = 99.99m,
            Quantity = 2
        };

        await client.PostAsJsonAsync($"/api/v1/cart/{cartId}/items", itemToAdd);

        Factory.MockRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cartId &&
            c.Items.Count == 1 &&
            c.Items[0].Quantity == 4
        )), Times.Once);
    }

    [Fact]
    public async Task DeleteCartItem_ShouldRemoveItemFromCart()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test Product",
            Price = 99.99m,
            Quantity = 1
        });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId))
            .ReturnsAsync(existingCart);

        var deleteResponse = await client.DeleteAsync($"/api/v1/cart/{cartId}/items/1");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        Factory.MockRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cartId &&
            c.Items.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task AddCartItem_ShouldAddMultipleDifferentItems()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem { Id = 1, Name = "Product 1", Price = 10.00m, Quantity = 1 });
        existingCart.AddItem(new CartItem { Id = 2, Name = "Product 2", Price = 20.00m, Quantity = 2 });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCart);

        var newItem = new CartItem { Id = 3, Name = "Product 3", Price = 30.00m, Quantity = 3 };

        await client.PostAsJsonAsync($"/api/v1/cart/{cartId}/items", newItem);

        Factory.MockRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cartId &&
            c.Items.Count == 3 &&
            c.Items.Any(i => i.Id == 1 && i.Name == "Product 1" && i.Price == 10.00m && i.Quantity == 1) &&
            c.Items.Any(i => i.Id == 2 && i.Name == "Product 2" && i.Price == 20.00m && i.Quantity == 2) &&
            c.Items.Any(i => i.Id == 3 && i.Name == "Product 3" && i.Price == 30.00m && i.Quantity == 3)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCartInfo_ShouldReturnCart_WhenCartExistsInRepository()
    {
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId };
        existingCart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Existing Product",
            Price = 50.00m,
            Quantity = 5,
            Image = new ImageInfo(
                Url: new Uri("https://example.com/existing.jpg"),
                AltText: "Existing Image"
            )
        });
        existingCart.AddItem(new CartItem
        {
            Id = 2,
            Name = "Another Product",
            Price = 75.00m,
            Quantity = 3
        });

        Factory.MockRepository.Setup(r => r.GetAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCart);

        var response = await client.GetAsync($"/api/v1/cart/{cartId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(cart);
        Assert.Equal(cartId, cart.Id);
        Assert.Equal(2, cart.Items.Count);

        var firstItem = cart.Items.FirstOrDefault(i => i.Id == 1);
        Assert.NotNull(firstItem);
        Assert.Equal("Existing Product", firstItem.Name);
        Assert.Equal(50.00m, firstItem.Price);
        Assert.Equal(5, firstItem.Quantity);
        Assert.NotNull(firstItem.Image);
        Assert.Equal("https://example.com/existing.jpg", firstItem.Image.Url);

        var secondItem = cart.Items.FirstOrDefault(i => i.Id == 2);
        Assert.NotNull(secondItem);
        Assert.Equal("Another Product", secondItem.Name);
        Assert.Equal(75.00m, secondItem.Price);
        Assert.Equal(3, secondItem.Quantity);
        Assert.Null(secondItem.Image);
    }
}
