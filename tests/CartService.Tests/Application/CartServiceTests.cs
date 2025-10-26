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
    public async Task GetItemsAsync_WhenCartDoesNotExist_ShouldReturnEmpty()
    {
        var emptyCartId = Guid.NewGuid();
        var actualItems = await service.GetItemsAsync(emptyCartId);

        Assert.NotNull(actualItems);
        Assert.Empty(actualItems);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task GetItemsAsync_WhenCartExists_ShouldReturnAllItems(int itemCount)
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
    public async Task AddItemAsync_WhenCartDoesNotExist_ShouldCreateCartAndAddItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var item = cart.Items[0];
        await service.AddItemAsync(cart.Id, item);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id &&
            c.Items.Count == 1 &&
            c.Items[0].Id == item.Id &&
            c.Items[0].Name == item.Name &&
            c.Items[0].Price == item.Price &&
            c.Items[0].Quantity == item.Quantity
        )), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WhenCartExists_ShouldAddNewItem()
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
    public async Task AddItemAsync_WhenItemExists_ShouldIncreaseQuantity()
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
    public async Task AddItemsAsync_WhenCartDoesNotExist_ShouldCreateCartAndAddItems(int itemCount)
    {
        Cart? savedCart = null;
        repositoryMock.Setup(r => r.SaveAsync(It.IsAny<Cart>()))
            .Callback<Cart>(c => savedCart = c)
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
    public async Task AddItemsAsync_WhenCartExists_ShouldAddNewItems()
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
    public async Task RemoveItemAsync_WhenCartDoesNotExist_ShouldNotSave()
    {
        var cartId = Guid.NewGuid();
        var item = new CartItem
        {
            Id = 1,
            Name = "Item",
            Price = 10m,
            Quantity = 1
        };

        await service.RemoveItemAsync(cartId, item);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenItemDoesNotExist_ShouldNotSave()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var nonExistentItem = new CartItem
        {
            Id = 999,
            Name = "NonExistent",
            Price = 10m,
            Quantity = 1
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, nonExistentItem);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenQuantityEqualsItemQuantity_ShouldRemoveItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1);
        var itemToRemove = new CartItem
        {
            Id = cart.Items[0].Id,
            Name = cart.Items[0].Name,
            Price = cart.Items[0].Price,
            Quantity = cart.Items[0].Quantity
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, itemToRemove);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenQuantityLessThanItemQuantity_ShouldDecreaseQuantity()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1, minQuantity: 500);
        var itemToRemove = new CartItem
        {
            Id = cart.Items[0].Id,
            Name = cart.Items[0].Name,
            Price = cart.Items[0].Price,
            Quantity = cart.Items[0].Quantity - 1
        };

        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, itemToRemove);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == cart.Id
            && c.Items.Count == 1
            && c.Items[0].Quantity == 1
        )), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenQuantityGreaterThanItemQuantity_ShouldRemoveItem()
    {
        var cart = TestHelper.CreateCartWithRandomItems(1, maxQuantity: 500);
        var itemToRemove = new CartItem
        {
            Id = cart.Items[0].Id,
            Name = cart.Items[0].Name,
            Price = cart.Items[0].Price,
            Quantity = cart.Items[0].Quantity + 1
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemAsync(cart.Id, itemToRemove);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task RemoveItemsAsync_WhenCartDoesNotExist_ShouldNotSave()
    {
        var cartId = Guid.NewGuid();
        var items = new List<CartItem>
        {
            new() { Id = 1, Name = "Item1", Price = 10m, Quantity = 1 },
            new() { Id = 2, Name = "Item2", Price = 20m, Quantity = 2 }
        };

        await service.RemoveItemsAsync(cartId, items);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemsAsync_WhenCartExists_ShouldRemoveItems()
    {
        var cart = TestHelper.CreateCartWithRandomItems(3);
        var itemsToRemove = new List<CartItem>
        {
            new()
            {
                Id = cart.Items[0].Id,
                Name = cart.Items[0].Name,
                Price = cart.Items[0].Price,
                Quantity = cart.Items[0].Quantity
            },
            new()
            {
                Id = cart.Items[1].Id,
                Name = cart.Items[1].Name,
                Price = cart.Items[1].Price,
                Quantity = cart.Items[1].Quantity
            }
        };
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.RemoveItemsAsync(cart.Id, itemsToRemove);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_WhenCartDoesNotExist_ShouldNotSave()
    {
        var cartId = Guid.NewGuid();

        await service.ClearAsync(cartId);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task ClearAsync_WhenCartIsEmpty_ShouldNotSave()
    {
        var cart = TestHelper.CreateCartWithRandomItems(0);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.ClearAsync(cart.Id);

        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task ClearAsync_WhenCartHasItems_ShouldRemoveAllItems(int itemCount)
    {
        var cart = TestHelper.CreateCartWithRandomItems(itemCount);
        repositoryMock.Setup(r => r.GetAsync(cart.Id)).ReturnsAsync(cart);

        await service.ClearAsync(cart.Id);

        repositoryMock.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Id == cart.Id && c.Items.Count == 0)), Times.Once);
    }
}
