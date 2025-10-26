using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.Tests.Persistence;

[Collection(nameof(DatabaseFixture))]
public class ProductRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IServiceScope scope = fixture.ServiceProvider.CreateScope();
    private ICategoryRepository categoryRepository;
    private IProductRepository productRepository;

    public Task InitializeAsync()
    {
        categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        using (scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            await DatabaseFixture.ResetDatabaseAsync(context);
        }
    }

    private async Task<Category> CreateCategoryAsync()
    {
        var category = CategoryRepositoryTests.RandomCategory;
        await categoryRepository.AddAsync(category);
        return category;
    }

    [Theory]
    [InlineData("Laptop", "High-performance laptop", 999.99, 10)]
    [InlineData("Phone", "Latest smartphone", 699.99, 20)]
    public async Task AddGetAsync_ShouldPersistProduct(string name, string description, decimal price, int amount)
    {
        var category = await CreateCategoryAsync();
        var product = new Product
        {
            Name = name,
            Description = description,
            ImageUrl = new Uri("https://example.com/test.jpg"),
            Category = category,
            Price = price,
            Amount = amount
        };

        await productRepository.AddAsync(product);
        var result = await productRepository.GetAsync(product.Id);

        Assert.NotNull(result);
        Assert.NotEqual(default, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.Equal(price, result.Price);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(product.ImageUrl, result.ImageUrl);
        Assert.NotNull(result.Category);
        Assert.Equal(category.Id, result.Category.Id);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllProducts()
    {
        var category = await CreateCategoryAsync();

        var products = new List<Product>
        {
            new() { Name = "T-Shirt", Category = category, Price = 19.99m, Amount = 50 },
            new() { Name = "Football", Category = category, Price = 29.99m, Amount = 15 }
        };

        await productRepository.AddRangeAsync(products);
        var result = await productRepository.ListAsync();

        Assert.NotNull(result);
        Assert.Equal(products.Count, result.Count);
        foreach (var product in products)
        {
            var found = result.First(p => p.Name == product.Name);
            Assert.Equal(product.Price, found.Price);
            Assert.Equal(product.Amount, found.Amount);
            Assert.NotNull(found.Category);
        }
    }

    [Theory]
    [InlineData("Smartphone", "Updated Smartphone", 899.99, 5)]
    [InlineData("Tablet", "Updated Tablet", 499.99, 8)]
    public async Task UpdateAsync_ShouldModifyProduct(string name, string newName, decimal newPrice, int newAmount)
    {
        var category = await CreateCategoryAsync();

        var product = new Product
        {
            Name = name,
            Description = "Test description",
            ImageUrl = new Uri("https://example.com/original.jpg"),
            Category = category,
            Price = 100,
            Amount = 1
        };

        await productRepository.AddAsync(product);

        product.Name = newName;
        product.Price = newPrice;
        product.Amount = newAmount;
        product.ImageUrl = new Uri("https://example.com/updated.jpg");

        await productRepository.UpdateAsync(product);
        var result = await productRepository.GetAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(newName, result.Name);
        Assert.Equal(newPrice, result.Price);
        Assert.Equal(newAmount, result.Amount);
        Assert.Equal(product.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        var category = await CreateCategoryAsync();
        var product = new Product
        {
            Name = "Toy Car",
            Category = category,
            Price = 14.99m,
            Amount = 100
        };

        await productRepository.AddAsync(product);
        await productRepository.DeleteAsync(product.Id);

        var result = await productRepository.GetAsync(product.Id);
        Assert.Null(result);
    }
}
