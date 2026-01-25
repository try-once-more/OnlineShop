using System.Linq.Expressions;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CatalogService.Infrastructure.IntegrationTests.Persistence;

[Collection(nameof(DatabaseFixture))]
public class CategoryRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IServiceScope scope = fixture.ServiceProvider.CreateScope();
    private ICategoryRepository categoryRepository;

    public Task InitializeAsync()
    {
        categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
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


    internal static Category RandomCategory => Random.Shared.Next(2) == 0
    ? RandomCategoryWithUrl
    : RandomCategoryWithoutUrl;

    internal static Category RandomCategoryWithUrl => new()
    {
        Name = $"{Guid.NewGuid():N}",
        ImageUrl = new Uri($"https://example.com/{Guid.NewGuid():N}")
    };

    internal static Category RandomCategoryWithoutUrl => new() { Name = $"{Guid.NewGuid():N}" };

    internal static IList<Category> CreateRandomCategories(int count) =>
        [.. Enumerable.Range(0, count).Select(_ => RandomCategory)];

    [Theory()]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddGetAsync_ShouldPersistCategory(bool withUrl)
    {
        var category = withUrl
            ? RandomCategoryWithUrl
            : RandomCategoryWithoutUrl;

        await categoryRepository.AddAsync(category);
        var result = await categoryRepository.GetAsync(category.Id);

        Assert.NotNull(result);
        Assert.NotEqual(default, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public async Task AddGetAsync_ShouldPersistCategoryWithHierarchy()
    {
        var root = new Category { Name = "Root" };
        var child = new Category { Name = "Child", ParentCategory = root };
        var grandChild = new Category { Name = "GrandChild", ParentCategory = child };

        await categoryRepository.AddAsync(root);
        await categoryRepository.AddAsync(child);
        await categoryRepository.AddAsync(grandChild);

        var result = await categoryRepository.GetAsync(grandChild.Id);

        Assert.NotNull(result);
        Assert.Equal("GrandChild", result.Name);
        Assert.NotNull(result.ParentCategory);
        Assert.Equal("Child", result.ParentCategory!.Name);
        Assert.NotNull(result.ParentCategory.ParentCategory);
        Assert.Equal("Root", result.ParentCategory.ParentCategory!.Name);
        Assert.Null(result.ParentCategory.ParentCategory.ParentCategory);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(55)]
    public async Task ListAsync_ShouldReturnCategories(int count)
    {
        var categories = CreateRandomCategories(count);
        await categoryRepository.AddRangeAsync(categories);

        var results = await categoryRepository.ListAsync();

        Assert.NotNull(results);
        Assert.Equal(categories.Count, results.Count);
        foreach (var category in categories)
        {
            var result = results.First(c => c.Id == category.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.ImageUrl, result.ImageUrl);
        }
    }

    [Fact]
    public async Task ListAsync_ShouldApplyFilter()
    {
        var categories = CreateRandomCategories(50);
        foreach (var category in categories.Take(15))
            category.Name = $"Duplicate_{Guid.NewGuid():N}";

        await categoryRepository.AddRangeAsync(categories);

        const int expectedCount = 10;
        Expression<Func<Category, bool>> filter = c => c.Name.Contains("Duplicate");
        var expectedOrder = categories.Where(filter.Compile()).OrderBy(c => c.Name).Take(expectedCount).Select(c => c.Id);

        var sortMock = new Mock<ISort<Category>>();
        sortMock
            .Setup(s => s.Apply(It.IsAny<IQueryable<Category>>(), It.IsAny<IOrderedQueryable<Category>?>()))
            .Returns<IQueryable<Category>, IOrderedQueryable<Category>?>((query, _) => query.OrderBy(c => c.Name));

        var options = new QueryOptions<Category>
        {
            Filter = filter,
            OrderBy = [sortMock.Object],
            Skip = 0,
            Take = expectedCount
        };

        var result = await categoryRepository.ListAsync(options);

        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.Equal(expectedOrder, result.Select(c => c.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_ShouldModifyCategory(bool withUrl)
    {
        var category = withUrl
            ? RandomCategoryWithUrl
            : RandomCategoryWithoutUrl;

        await categoryRepository.AddAsync(category);

        category.Name = "Updated Name";
        category.ImageUrl = new Uri("https://example.com/updated.jpg");

        await categoryRepository.UpdateAsync(category);
        var result = await categoryRepository.GetAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.ImageUrl, result.ImageUrl);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_ShouldRemoveCategory(bool withUrl)
    {
        var category = withUrl
            ? RandomCategoryWithUrl
            : RandomCategoryWithoutUrl;

        await categoryRepository.AddAsync(category);

        await categoryRepository.DeleteAsync(category.Id);
        var result = await categoryRepository.GetAsync(category.Id);

        Assert.Null(result);
    }
}
