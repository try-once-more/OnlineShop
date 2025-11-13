using System.Net;
using System.Net.Http.Json;

namespace CatalogService.API.Tests.Controllers;

[Collection(nameof(CatalogApiFactory))]
public class CategoriesControllerTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    [Fact]
    public async Task GetCategories_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        var categories = await _client.GetFromJsonAsync<CategoryDto[]>("/api/v1/categories");
        Assert.NotNull(categories);
        Assert.Empty(categories);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreatedCategory()
    {
        var command = new AddCategoryRequestDto
        {
            Name = "Electronics",
            ImageUrl = new Uri("https://example.com/electronics.jpg")
        };
        var response = await _client.PostAsJsonAsync("/api/v1/categories", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.NotEqual(0, category.Id);
        Assert.Equal(command.Name, category.Name);
        Assert.Equal(command.ImageUrl, category.ImageUrl);
        Assert.NotNull(response.Headers.Location);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenNameIsInvalid(string? invalidName)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/categories", new AddCategoryRequestDto { Name = invalidName });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenNameExceedsMaxLength()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/categories", new AddCategoryRequestDto { Name = new string('a', 51) });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
    {
        var created = await CreateCategoryAsync("Books");
        var category = await _client.GetFromJsonAsync<CategoryDto>($"/api/v1/categories/{created.Id}");
        Assert.NotNull(category);
        Assert.Equal(created.Id, category.Id);
        Assert.Equal(created.Name, category.Name);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/categories/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnPaginatedResults()
    {
        for (int i = 1; i <= 15; i++)
            await CreateCategoryAsync($"Category {i}");

        var page1 = await _client.GetFromJsonAsync<CategoryDto[]>("/api/v1/categories?pageNumber=1&pageSize=10");
        var page2 = await _client.GetFromJsonAsync<CategoryDto[]>("/api/v1/categories?pageNumber=2&pageSize=10");
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(10, page1.Length);
        Assert.Equal(5, page2.Length);
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateCategoryName()
    {
        var created = await CreateCategoryAsync("Original Name");
        var response = await _client.PatchAsJsonAsync($"/api/v1/categories/{created.Id}", new { name = "Updated Name" });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await _client.GetFromJsonAsync<CategoryDto>($"/api/v1/categories/{created.Id}");
        Assert.Equal("Updated Name", updated!.Name);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var response = await _client.PatchAsJsonAsync("/api/v1/categories/999999", new { name = "New Name" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ShouldDeleteCategory()
    {
        var created = await CreateCategoryAsync("To Delete");
        var deleteResponse = await _client.DeleteAsync($"/api/v1/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getResponse = await _client.GetAsync($"/api/v1/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithParentCategory_ShouldCreateHierarchy()
    {
        var parent = await CreateCategoryAsync("Parent Category");
        var child = await CreateCategoryAsync("Child Category", parentCategoryId: parent.Id);
        Assert.Equal(parent.Id, child.ParentCategoryId);
    }

    [Fact]
    public async Task DeleteCategory_WithForceFlag_ShouldDeleteCategoryAndChildren()
    {
        var parent = await CreateCategoryAsync("Parent");
        await CreateCategoryAsync("Child", parentCategoryId: parent.Id);
        var deleteResponse = await _client.DeleteAsync($"/api/v1/categories/{parent.Id}?force=true");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getParentResponse = await _client.GetAsync($"/api/v1/categories/{parent.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getParentResponse.StatusCode);
    }

    private async Task<CategoryDto> CreateCategoryAsync(string name = "Test Category", Uri? imageUrl = null, int? parentCategoryId = null)
        => await CreateCategoryAsync(_client, name, imageUrl, parentCategoryId);

    internal async static Task<CategoryDto> CreateCategoryAsync(HttpClient client, string name = "Test Category", Uri? imageUrl = null, int? parentCategoryId = null)
    {
        var command = new AddCategoryRequestDto { Name = name, ImageUrl = imageUrl, ParentCategoryId = parentCategoryId };
        var response = await client.PostAsJsonAsync("/api/v1/categories", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>()!;
    }
}

internal class AddCategoryRequestDto
{
    public string Name { get; set; }
    public Uri? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
}

internal class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Uri? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public object? Links { get; set; }
}
