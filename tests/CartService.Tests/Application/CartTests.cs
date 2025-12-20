using CartService.Application.Entities;

namespace CartService.Tests.Application;

public class CartTests
{
    [Fact]
    public void ShouldCreateEmptyCart()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddItem_WhenNull_ShouldThrowException()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.Throws<ArgumentNullException>(() => cart.AddItem(null));
    }

    [Fact]
    public void AddItem_ShouldAddNewItemToCart()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var item = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        };

        cart.AddItem(item);

        Assert.Single(cart.Items);
        Assert.Equal(item.Id, cart.Items[0].Id);
        Assert.Equal(5, cart.Items[0].Quantity);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void AddItem_ShouldAddMultipleDifferentItems(int itemCount)
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        for (int i = 1; i <= itemCount; i++)
        {
            cart.AddItem(new CartItem
            {
                Id = i,
                Name = $"Item{i}",
                Price = 10m * i,
                Quantity = i
            });
        }

        Assert.Equal(itemCount, cart.Items.Count);
    }

    [Fact]
    public void AddItem_ShouldIncreaseQuantityForExistingItem()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var item = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        };
        cart.AddItem(item);

        var sameItem = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 3
        };
        cart.AddItem(sameItem);

        Assert.Single(cart.Items);
        Assert.Equal(8, cart.Items[0].Quantity);
    }

    [Fact]
    public void RemoveItem_ShouldReturnFalseForEmptyCart()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        var result = cart.RemoveItem(1);

        Assert.False(result);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_ShouldReturnFalseWhenItemNotFound()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        });

        var result = cart.RemoveItem(999);

        Assert.False(result);
        Assert.Single(cart.Items);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveExistingItem()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var item = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        };
        cart.AddItem(item);

        var result = cart.RemoveItem(item.Id);

        Assert.True(result);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveOnlySpecifiedItem()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test1",
            Price = 10m,
            Quantity = 5
        });
        cart.AddItem(new CartItem
        {
            Id = 2,
            Name = "Test2",
            Price = 20m,
            Quantity = 3
        });

        var result = cart.RemoveItem(1);

        Assert.True(result);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Id);
    }

    [Fact]
    public void Clear_ShouldReturnFalseForEmptyCart()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        var result = cart.Clear();

        Assert.False(result);
        Assert.Empty(cart.Items);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Clear_ShouldRemoveAllItems(int itemCount)
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        for (int i = 1; i <= itemCount; i++)
        {
            cart.AddItem(new CartItem
            {
                Id = i,
                Name = $"Item{i}",
                Price = 10m,
                Quantity = 1
            });
        }

        var result = cart.Clear();

        Assert.True(result);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameId()
    {
        var cartId = Guid.NewGuid();
        var cart1 = new Cart { Id = cartId };
        cart1.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        });

        var cart2 = new Cart { Id = cartId };

        Assert.True(cart1.Equals(cart2));
        Assert.True(cart1.Equals(cart2));
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentId()
    {
        var cart1 = new Cart { Id = Guid.NewGuid() };
        var cart2 = new Cart { Id = Guid.NewGuid() };

        Assert.False(cart1.Equals(cart2));
        Assert.True(cart1 != cart2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValueForSameId()
    {
        var cartId = Guid.NewGuid();
        var cart1 = new Cart { Id = cartId };
        var cart2 = new Cart { Id = cartId };

        Assert.Equal(cart1.GetHashCode(), cart2.GetHashCode());
    }
}
