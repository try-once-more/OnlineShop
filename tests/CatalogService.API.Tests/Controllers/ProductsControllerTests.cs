using CatalogService.Application.Products;
using System.Net;
using System.Net.Http.Json;

namespace CatalogService.API.Tests.Controllers;

[Collection(nameof(CatalogApiFactory))]
public class ProductsControllerTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    private async Task<ProductDto> CreateProductAsyncInternal(string name, int categoryId, decimal price = 10m, int amount = 1)
    {
        var command = new AddProductCommand
        {
            Name = name,
            CategoryId = categoryId,
            Price = price,
            Amount = amount
        };
        var response = await _client.PostAsJsonAsync("/api/v1/products", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>()!;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        var products = await _client.GetFromJsonAsync<ProductDto[]>("/api/v1/products");
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedProduct()
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var command = new AddProductCommand
        {
            Name = "Laptop",
            Description = "High-performance laptop",
            ImageUrl = new Uri("https://example.com/laptop.jpg"),
            CategoryId = category.Id,
            Price = 999.99m,
            Amount = 10
        };
        var response = await _client.PostAsJsonAsync("/api/v1/products", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(command.Name, product.Name);
        Assert.Equal(command.Description, product.Description);
        Assert.Equal(command.ImageUrl.ToString(), product.ImageUrl);
        Assert.Equal(command.Price, product.Price);
        Assert.Equal(command.Amount, product.Amount);
        Assert.Equal(category.Id, product.CategoryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenNameIsInvalid(string? invalidName)
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var response = await _client.PostAsJsonAsync("/api/v1/products", new AddProductCommand
        {
            Name = invalidName!,
            CategoryId = category.Id,
            Price = 99.99m,
            Amount = 5
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenPriceOrAmountIsInvalid(decimal invalidValue)
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var responsePrice = await _client.PostAsJsonAsync("/api/v1/products", new AddProductCommand
        {
            Name = "Product",
            CategoryId = category.Id,
            Price = invalidValue,
            Amount = 5
        });
        Assert.Equal(HttpStatusCode.BadRequest, responsePrice.StatusCode);

        var responseAmount = await _client.PostAsJsonAsync("/api/v1/products", new AddProductCommand
        {
            Name = "Product",
            CategoryId = category.Id,
            Price = 10m,
            Amount = (int)invalidValue
        });
        Assert.Equal(HttpStatusCode.BadRequest, responseAmount.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/products", new AddProductCommand
        {
            Name = "Product",
            CategoryId = 999999,
            Price = 99.99m,
            Amount = 5
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var created = await CreateProductAsync("Phone", category.Id, 699.99m, 20);
        var product = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{created.Id}");
        Assert.NotNull(product);
        Assert.Equal(created.Id, product.Id);
        Assert.Equal(created.Name, product.Name);
        Assert.Equal(created.Price, product.Price);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/products/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedResults()
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        for (int i = 1; i <= 15; i++) await CreateProductAsync($"Product {i}", category.Id, 10m * i, i);

        var page1 = await _client.GetFromJsonAsync<ProductDto[]>("/api/v1/products?pageNumber=1&pageSize=10");
        var page2 = await _client.GetFromJsonAsync<ProductDto[]>("/api/v1/products?pageNumber=2&pageSize=10");
        Assert.Equal(10, page1!.Length);
        Assert.Equal(5, page2!.Length);
    }

    [Fact]
    public async Task GetProducts_ShouldFilterByCategory()
    {
        var cat1 = await CategoriesControllerTests.CreateCategoryAsync(_client, "Category 1");
        var cat2 = await CategoriesControllerTests.CreateCategoryAsync(_client, "Category 2");

        for (int i = 1; i <= 5; i++) await CreateProductAsync($"Product Cat1-{i}", cat1.Id);
        for (int i = 1; i <= 3; i++) await CreateProductAsync($"Product Cat2-{i}", cat2.Id);

        var products = await _client.GetFromJsonAsync<ProductDto[]>($"/api/v1/products?categoryId={cat1.Id}");
        Assert.Equal(5, products!.Length);
        Assert.All(products, p => Assert.Equal(cat1.Id, p.CategoryId));
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateFields()
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var created = await CreateProductAsync("Original", category.Id, 100m, 10);

        var updateCommand = new { name = "Updated", description = "Updated Desc", price = 150m, amount = 20 };
        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{created.Id}", updateCommand);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{created.Id}");
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Updated Desc", updated.Description);
        Assert.Equal(150m, updated.Price);
        Assert.Equal(20, updated.Amount);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response = await _client.PatchAsJsonAsync("/api/v1/products/999999", new { name = "New Name" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ShouldDeleteProduct()
    {
        var category = await CategoriesControllerTests.CreateCategoryAsync(_client);
        var created = await CreateProductAsync("To Delete", category.Id, 50m, 5);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateCategory()
    {
        var cat1 = await CategoriesControllerTests.CreateCategoryAsync(_client, "Category 1");
        var cat2 = await CategoriesControllerTests.CreateCategoryAsync(_client, "Category 2");
        var created = await CreateProductAsync("Product", cat1.Id);

        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{created.Id}", new { categoryId = cat2.Id });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail(await response.Content.ReadAsStringAsync());
        }
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{created.Id}");
        Assert.Equal(cat2.Id, updated!.CategoryId);
    }

    private async Task<ProductDto> CreateProductAsync(string name, int categoryId, decimal price = 10m, int amount = 1)
    {
        var command = new AddProductCommand
        {
            Name = name,
            CategoryId = categoryId,
            Price = price,
            Amount = amount
        };
        var response = await _client.PostAsJsonAsync("/api/v1/products", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>()!;
    }
}

internal class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
}
