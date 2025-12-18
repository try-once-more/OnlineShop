using CartService.Application.Abstractions;
using CartService.Application.Entities;
using CartService.Infrastructure;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace CartService.Tests.Infrastructure;

public class LiteCartRepositoryFixture : IAsyncLifetime
{
    private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"{nameof(LiteCartRepositoryTests)}.db");

    public ServiceProvider ServiceProvider { get; private set; }

    public LiteCartRepositoryFixture()
    {
        ServiceProvider = new ServiceCollection()
            .Configure<CartDatabaseOptions>(options => options.CartDatabase = dbPath)
            .AddCartServiceInfrastructure()
            .BuildServiceProvider();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        if (File.Exists(dbPath))
            File.Delete(dbPath);

        return ServiceProvider.DisposeAsync().AsTask();
    }
}

[Collection(nameof(LiteCartRepositoryTests))]
public class LiteCartRepositoryTests(LiteCartRepositoryFixture fixture)
    : IAsyncLifetime, IClassFixture<LiteCartRepositoryFixture>
{
    private readonly IServiceScope scope = fixture.ServiceProvider.CreateScope();
    private ICartRepository categoryRepository;

    public Task InitializeAsync()
    {
        categoryRepository = scope.ServiceProvider.GetRequiredService<ICartRepository>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        var db = scope.ServiceProvider.GetRequiredService<ILiteDatabase>();
        db.GetCollection<Cart>("carts").DeleteAll();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAsync_WhenCartDoesNotExist_ShouldReturnNull()
    {
        var cartId = Guid.NewGuid();
        var cart = await categoryRepository.GetAsync(cartId);
        Assert.Null(cart);
    }

    [Fact]
    public async Task SaveAsync_WhenCartIsEmpty_ShouldPersistCartWithNoItems()
    {
        var emptyCart = TestHelper.CreateCartWithRandomItems(0);

        await categoryRepository.SaveAsync(emptyCart);
        var actualCart = await categoryRepository.GetAsync(emptyCart.Id);

        Assert.NotNull(actualCart);
        Assert.Empty(actualCart.Items);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task SaveAsync_WhenCartIsNew_ShouldPersistCartAndItems(int itemCount)
    {
        var expectedCart = TestHelper.CreateCartWithRandomItems(itemCount);

        await categoryRepository.SaveAsync(expectedCart);
        var actualCart = await categoryRepository.GetAsync(expectedCart.Id);

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
        await categoryRepository.SaveAsync(expectedCart);

        expectedCart = TestHelper.CreateCartWithRandomItems(expectedCart.Id, 2);

        await categoryRepository.SaveAsync(expectedCart);

        var actualCart = await categoryRepository.GetAsync(expectedCart.Id);

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
