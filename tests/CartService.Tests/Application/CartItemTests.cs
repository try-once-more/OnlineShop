using CartService.Application;
using CartService.Application.Entities;

namespace CartService.Tests.Application;

public class CartItemTests
{
    private static CartItem ValidItem => new() { Id = 1, Name = "Test", Price = 10m, Quantity = 10 };

    [Fact]
    public void Create_WhenValid_ShouldCreateCartItem()
    {
        var item = new CartItem { Id = 1, Name = "Test Item", Price = 10.5m, Quantity = 5 };

        Assert.Equal(1, item.Id);
        Assert.Equal("Test Item", item.Name);
        Assert.Equal(10.5m, item.Price);
        Assert.Equal(5, item.Quantity);
        Assert.Null(item.Image);
    }

    [Fact]
    public void Create_WhenDefaultId_ShouldThrowException() =>
        Assert.Throws<EntityIdInvalidException>(() => new CartItem { Id = default, Name = "Test", Price = 10m, Quantity = 1 });

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameInvalid_ShouldThrowException(string name) =>
        Assert.Throws<CartItemNameInvalidException>(() => new CartItem { Id = 1, Name = name, Price = 10m, Quantity = 1 });

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Create_WhenPriceInvalid_ShouldThrowException(decimal price) =>
        Assert.Throws<CartItemPriceInvalidException>(() => new CartItem { Id = 1, Name = "Test", Price = price, Quantity = 1 });

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Create_WhenQuantityInvalid_ShouldThrowException(int quantity) =>
        Assert.Throws<CartItemQuantityInvalidException>(() => new CartItem { Id = 1, Name = "Test", Price = 10m, Quantity = quantity });

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(int.MaxValue - 1)]
    public void IncreaseQuantity_WhenValueIsValid_ShouldIncreaseQuantity(int increaseBy)
    {
        var item = new CartItem { Id = 1, Name = "Test", Price = 10m, Quantity = 1 };
        var expectedQuantity = 1 + increaseBy;

        item.IncreaseQuantity(increaseBy);

        Assert.Equal(expectedQuantity, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void IncreaseQuantity_WhenValueIsInvalid_ShouldThrowException(int value) =>
        Assert.Throws<CartItemQuantityInvalidException>(() => ValidItem.IncreaseQuantity(value));

    [Theory]
    [InlineData(2147483638)]
    [InlineData(int.MaxValue)]
    public void IncreaseQuantity_WhenResultingQuantityIsInvalid_ShouldThrowException(int value) =>
        Assert.Throws<CartItemQuantityInvalidException>(() => ValidItem.IncreaseQuantity(value));

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    public void DecreaseQuantity_WhenValueIsLessThanCurrentQuantity_ShouldDecreaseQuantity(int decreaseBy)
    {
        var item = new CartItem { Id = 1, Name = "Test", Price = 10m, Quantity = 10 };
        var expectedQuantity = 10 - decreaseBy;

        item.DecreaseQuantity(decreaseBy);

        Assert.Equal(expectedQuantity, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void DecreaseQuantity_WhenValueIsInvalid_ShouldThrowException(int value) =>
    Assert.Throws<CartItemQuantityInvalidException>(() => ValidItem.IncreaseQuantity(value));

    [Theory]
    [InlineData(11)]
    [InlineData(int.MaxValue)]
    public void DecreaseQuantity_WhenResultingQuantityIsInvalid_ShouldThrowException(int value) =>
        Assert.Throws<CartItemQuantityInvalidException>(() => ValidItem.DecreaseQuantity(value));

    [Fact]
    public void Equals_WhenItemsHaveSameId_ShouldReturnTrue()
    {
        var item1 = new CartItem { Id = 1, Name = "Test1", Price = 10m, Quantity = 5 };
        var item2 = new CartItem { Id = 1, Name = "Test2", Price = 20m, Quantity = 10 };

        Assert.True(item1.Equals(item2));
        Assert.True(item1 == item2);
    }

    [Fact]
    public void Equals_WhenItemsHaveDifferentId_ShouldReturnFalse()
    {
        var item1 = new CartItem { Id = 1, Name = "Test", Price = 10m, Quantity = 5 };
        var item2 = new CartItem { Id = 2, Name = "Test", Price = 10m, Quantity = 5 };

        Assert.False(item1.Equals(item2));
        Assert.True(item1 != item2);
    }

    [Fact]
    public void GetHashCode_WhenItemsHaveSameId_ShouldReturnSameHashCode()
    {
        var item1 = new CartItem { Id = 1, Name = "Test1", Price = 10m, Quantity = 5 };
        var item2 = new CartItem { Id = 1, Name = "Test2", Price = 20m, Quantity = 10 };

        Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
    }
}
