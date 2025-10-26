using CartService.Application.Entities;
using CartService.Infrastructure;
using LiteDB;

namespace CartService.Tests.Infrastructure;

public class LiteCartRepositoryFixture : IAsyncLifetime
{
    private string dbPath;
    internal ILiteDatabase Database { get; private set; }
    internal LiteCartRepository Repository { get; private set; }

    public Task InitializeAsync()
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"{nameof(LiteCartRepositoryTests)}.db");
        Database = new LiteDatabase(dbPath);
        Repository = new LiteCartRepository(Database);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Database?.Dispose();
        if (File.Exists(dbPath))
            File.Delete(dbPath);
        return Task.CompletedTask;
    }
}

[Collection(nameof(LiteCartRepositoryTests))]
public class LiteCartRepositoryTests(LiteCartRepositoryFixture fixture)
    : IAsyncLifetime, IClassFixture<LiteCartRepositoryFixture>
{
    public Task InitializeAsync()
    {
        fixture.Database.GetCollection<Cart>("carts").DeleteAll();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAsync_WhenCartDoesNotExist_ShouldReturnNull()
    {
        var cartId = Guid.NewGuid();
        var cart = await fixture.Repository.GetAsync(cartId);
        Assert.Null(cart);
    }

    [Fact]
    public async Task SaveAsync_WhenCartIsEmpty_ShouldPersistCartWithNoItems()
    {
        var emptyCart = TestHelper.CreateCartWithRandomItems(0);

        await fixture.Repository.SaveAsync(emptyCart);
        var actualCart = await fixture.Repository.GetAsync(emptyCart.Id);

        Assert.NotNull(actualCart);
        Assert.Empty(actualCart.Items);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task SaveAsync_WhenCartIsNew_ShouldPersistCartAndItems(int itemCount)
    {
        var expectedCart = TestHelper.CreateCartWithRandomItems(itemCount);

        await fixture.Repository.SaveAsync(expectedCart);
        var actualCart = await fixture.Repository.GetAsync(expectedCart.Id);

        Assert.NotNull(actualCart);
        Assert.Equal(expectedCart.Id, actualCart.Id);
        Assert.Equal(expectedCart.Items.Count, actualCart.Items.Count);

        for (int i = 0; i < expectedCart.Items.Count; i++)
        {
            var expectedItem = expectedCart.Items[i];
            var actualItem = actualCart.Items[i];

            Assert.Equal(expectedItem.Id, actualItem.Id);
            Assert.Equal(expectedItem.Name, actualItem.Name);
            Assert.Equal(expectedItem.Price, actualItem.Price);
            Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
            Assert.Equal(expectedItem.Image, actualItem.Image);
        }
    }

    [Fact]
    public async Task SaveAsync_WhenCartExists_ShouldUpdateItems()
    {
        var expectedCart = TestHelper.CreateCartWithRandomItems(3);
        await fixture.Repository.SaveAsync(expectedCart);

        expectedCart = TestHelper.CreateCartWithRandomItems(expectedCart.Id, 2);

        await fixture.Repository.SaveAsync(expectedCart);

        var actualCart = await fixture.Repository.GetAsync(expectedCart.Id);

        Assert.NotNull(actualCart);
        Assert.Equal(expectedCart.Items.Count, actualCart.Items.Count);

        for (int i = 0; i < expectedCart.Items.Count; i++)
        {
            var expectedItem = expectedCart.Items[i];
            var actualItem = actualCart.Items[i];

            Assert.Equal(expectedItem.Id, actualItem.Id);
            Assert.Equal(expectedItem.Name, actualItem.Name);
            Assert.Equal(expectedItem.Price, actualItem.Price);
            Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
            Assert.Equal(expectedItem.Image, actualItem.Image);
        }
    }
}
