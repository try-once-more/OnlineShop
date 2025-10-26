using CartService.Application.Entities;

namespace CartService.Tests.Application;

public class CartTests
{
    [Fact]
    public void Create_WhenDefault_ShouldCreateEmptyCart()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddItem_WhenItemIsNull_ShouldThrowArgumentNullException()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.Throws<ArgumentNullException>(() => cart.AddItem(null));
    }

    [Fact]
    public void AddItem_WhenCartIsEmpty_ShouldAddItem()
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
    public void AddItem_WhenMultipleUniqueItems_ShouldAddAllItems(int itemCount)
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
    public void AddItem_WhenItemAlreadyExists_ShouldIncreaseQuantity()
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

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    public void AddItem_WhenSameItemAddedMultipleTimes_ShouldAccumulateQuantity(int addCount, int quantityPerAdd)
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var expectedQuantity = addCount * quantityPerAdd;

        for (int i = 0; i < addCount; i++)
        {
            cart.AddItem(new CartItem
            {
                Id = 1,
                Name = "Test",
                Price = 10m,
                Quantity = quantityPerAdd
            });
        }

        Assert.Single(cart.Items);
        Assert.Equal(expectedQuantity, cart.Items[0].Quantity);
    }

    [Fact]
    public void RemoveItem_WhenItemIsNull_ShouldThrowArgumentNullException()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.Throws<ArgumentNullException>(() => cart.RemoveItem(null!));
    }

    [Fact]
    public void RemoveItem_WhenCartIsEmpty_ShouldReturnFalse()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var item = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        };

        var result = cart.RemoveItem(item);

        Assert.False(result);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_WhenItemDoesNotExist_ShouldReturnFalse()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        });

        var nonExistentItem = new CartItem
        {
            Id = 999,
            Name = "Other",
            Price = 20m,
            Quantity = 1
        };
        var result = cart.RemoveItem(nonExistentItem);

        Assert.False(result);
        Assert.Single(cart.Items);
    }

    [Fact]
    public void RemoveItem_WhenQuantityEqualsItemQuantity_ShouldRemoveItem()
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

        var itemToRemove = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 5
        };
        var result = cart.RemoveItem(itemToRemove);

        Assert.True(result);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_WhenQuantityGreaterThanItemQuantity_ShouldRemoveItem()
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

        var itemToRemove = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 10
        };
        var result = cart.RemoveItem(itemToRemove);

        Assert.True(result);
        Assert.Empty(cart.Items);
    }

    [Theory]
    [InlineData(10, 1)]
    [InlineData(10, 5)]
    [InlineData(10, 9)]
    public void RemoveItem_WhenQuantityLessThanItemQuantity_ShouldDecreaseQuantity(int initialQuantity, int removeQuantity)
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        var item = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = initialQuantity
        };
        cart.AddItem(item);

        var itemToRemove = new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = removeQuantity
        };
        var result = cart.RemoveItem(itemToRemove);

        Assert.True(result);
        Assert.Single(cart.Items);
        Assert.Equal(initialQuantity - removeQuantity, cart.Items[0].Quantity);
    }

    [Fact]
    public void Clear_WhenCartIsEmpty_ShouldReturnFalse()
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
    public void Clear_WhenCartHasItems_ShouldRemoveAllItemsAndReturnTrue(int itemCount)
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
    public void Clear_WhenCalledMultipleTimes_ShouldReturnFalseAfterFirstCall()
    {
        var cart = new Cart { Id = Guid.NewGuid() };
        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            Quantity = 1
        });

        var firstResult = cart.Clear();
        var secondResult = cart.Clear();

        Assert.True(firstResult);
        Assert.False(secondResult);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Items_WhenModified_ShouldReflectChanges()
    {
        var cart = new Cart { Id = Guid.NewGuid() };

        Assert.Empty(cart.Items);

        cart.AddItem(new CartItem
        {
            Id = 1,
            Name = "Test1",
            Price = 10m,
            Quantity = 1
        });
        Assert.Single(cart.Items);

        cart.AddItem(new CartItem
        {
            Id = 2,
            Name = "Test2",
            Price = 20m,
            Quantity = 2
        });
        Assert.Equal(2, cart.Items.Count);

        cart.RemoveItem(new CartItem
        {
            Id = 1,
            Name = "Test1",
            Price = 10m,
            Quantity = 1
        });
        Assert.Single(cart.Items);

        cart.Clear();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Equals_WhenCartsHaveSameId_ShouldReturnTrue()
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
        Assert.True(cart1 == cart2);
    }

    [Fact]
    public void Equals_WhenCartsHaveDifferentId_ShouldReturnFalse()
    {
        var cart1 = new Cart { Id = Guid.NewGuid() };
        var cart2 = new Cart { Id = Guid.NewGuid() };

        Assert.False(cart1.Equals(cart2));
        Assert.True(cart1 != cart2);
    }

    [Fact]
    public void GetHashCode_WhenCartsHaveSameId_ShouldReturnSameHashCode()
    {
        var cartId = Guid.NewGuid();
        var cart1 = new Cart { Id = cartId };
        var cart2 = new Cart { Id = cartId };

        Assert.Equal(cart1.GetHashCode(), cart2.GetHashCode());
    }
}
