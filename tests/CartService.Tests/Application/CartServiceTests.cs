using CartService.Application.Abstractions;
using CartService.Application.Entities;
using CartService.Application.Services;
using Moq;

namespace CartService.Tests.Application;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> repositoryMock = new();
    private readonly ICartService service;

    public CartServiceTests() => service = new CartAppService(repositoryMock.Object);

    [Fact]
    public async Task GetItemsAsync_ShouldReturnEmptyForNonExistentCart()
    {
        var emptyCartId = Guid.NewGuid();

        var actualItems = await service.GetItemsAsync(emptyCartId);

        Assert.NotNull(actualItems);
        Assert.Empty(actualItems);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task GetItemsAsync_ShouldReturnAllItemsFromCart(int itemCount)
    {
        var cart = TestHelper.CreateCartWithRandomItems(itemCount);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        var actualItems = await service.GetItemsAsync(cart.Id);

        Assert.NotNull(actualItems);
        Assert.Equal(cart.Items.Count, actualItems.Count);

        for (int i = 0; i < cart.Items.Count; i++)
        {
            var expectedItem = cart.Items[i];
            var actualItem = actualItems[i];

            Assert.Equal(expectedItem.Id, actualItem.Id);
            Assert.Equal(expectedItem.Name, actualItem.Name);
            Assert.Equal(expectedItem.Price, actualItem.Price);
            Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
            Assert.Equal(expectedItem.Image, actualItem.Image);
        }

        repositoryMock.Verify(r => r.GetAsync(cart.Id), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldCreateCartAndAddItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var item = cart.Items[0];

        await service.AddItemAsync(cart.Id, item);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id
            && c.Items.Count == 1
            && c.Items[0].Id == item.Id
            && c.Items[0].Name == item.Name
            && c.Items[0].Price == item.Price
            && c.Items[0].Quantity == item.Quantity
        )), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddNewItemToExistingCart()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var newItem = new CartItem
        {
            Id = 999,
            Name = "Added",
            Price = 5m,
            Quantity = 1
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.AddItemAsync(cart.Id, newItem);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id &&
            c.Items.Count == 2 &&
            c.Items.Any(i => i.Id == newItem.Id)
        )), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncreaseQuantityForExistingItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1, maxQuantity: 500);
        var addSame = new CartItem
        {
            Id = cart.Items[0].Id,
            Name = cart.Items[0].Name,
            Price = cart.Items[0].Price,
            Quantity = 3
        };
        var expectedQuantity = cart.Items[0].Quantity + addSame.Quantity;
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.AddItemAsync(cart.Id, addSame);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id &&
            c.Items.Count == 1 &&
            c.Items[0].Quantity == expectedQuantity
      )), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task AddItemsAsync_ShouldCreateCartAndAddMultipleItems(int itemCount)
    {
        Cart? savedCart = null;
        repositoryMock.Setup(r => r.SaveAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>((c, _) => savedCart = c)
            .Returns(Task.CompletedTask);
        var cart = TestHelper.CreateCartWithRandomItems(itemCount);

        await service.AddItemsAsync(cart.Id, cart.Items);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Once);

        Assert.NotNull(savedCart);
        Assert.Equal(cart.Id, savedCart.Id);
        Assert.Equal(cart.Items.Count, savedCart.Items.Count);
        for (int i = 0; i < cart.Items.Count; i++)
        {
            var expected = cart.Items[i];
            var actual = savedCart.Items[i];

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Price, actual.Price);
            Assert.Equal(expected.Quantity, actual.Quantity);
            Assert.Equal(expected.Image, actual.Image);
        }
    }

    [Fact]
    public async Task AddItemsAsync_ShouldAddNewItemsToExistingCart()
    {
        var cart = TestHelper.CreateCartWithRandomItems(2);
        var newItems = new List<CartItem>
        {
            new() { Id = 998, Name = "New1", Price = 5m, Quantity = 1 },
            new() { Id = 999, Name = "New2", Price = 10m, Quantity = 2 }
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.AddItemsAsync(cart.Id, newItems);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 4)), Times.Once);
    }

    [Fact]
    public async Task ChangeItemQuantityAsync_ShouldUpdateQuantity()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var itemId = cart.Items[0].Id;
        var newQuantity = 100;
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.ChangeItemQuantityAsync(cart.Id, itemId, newQuantity);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id &&
            c.Items[0].Quantity == newQuantity
        )), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldNotSaveForNonExistentCart()
    {
        var cartId = Guid.NewGuid();

        await service.RemoveItemAsync(cartId, 1);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldNotSaveWhenItemNotFound()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, 999);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveExistingItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var itemId = cart.Items[0].Id;
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, itemId);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task RemoveItemsAsync_ShouldNotSaveForNonExistentCart()
    {
        var cartId = Guid.NewGuid();
        var itemIds = new List<int> { 1, 2 };

        await service.RemoveItemsAsync(cartId, itemIds);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemsAsync_ShouldRemoveMultipleItems()
    {
        var cart = TestHelper.CreateCartWithRandomItems(3);
        var itemIdsToRemove = new List<int>
        {
            cart.Items[0].Id,
            cart.Items[1].Id
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemsAsync(cart.Id, itemIdsToRemove);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_ShouldNotSaveForNonExistentCart()
    {
        var cartId = Guid.NewGuid();

        await service.ClearAsync(cartId);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task ClearAsync_ShouldNotSaveForEmptyCart()
    {
        var cart = TestHelper.CreateCartWithRandomItems(0);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.ClearAsync(cart.Id);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task ClearAsync_ShouldRemoveAllItems(int itemCount)
    {
        var cart = TestHelper.CreateCartWithRandomItems(itemCount);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.ClearAsync(cart.Id);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 0)), Times.Once);
    }
}
