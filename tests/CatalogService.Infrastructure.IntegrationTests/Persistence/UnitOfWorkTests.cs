using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.IntegrationTests.Persistence;

[Trait("Category", "IntegrationTests")]
[Collection(nameof(DatabaseFixture))]
public class UnitOfWorkTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IServiceScope scope = fixture.ServiceProvider.CreateScope();
    private IUnitOfWork unitOfWork;

    public ValueTask InitializeAsync()
    {
        unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        using (scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            await DatabaseFixture.ResetDatabaseAsync(context);
        }
    }

    [Fact]
    public async Task CommitAsync_ShouldPersistChanges()
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        var category = new Category { Name = "CommitTest" };
        await unitOfWork.Categories.AddAsync(category);

        await transaction.CommitAsync();

        var saved = await unitOfWork.Categories.GetAsync(category.Id);
        Assert.NotNull(saved);
        Assert.Equal("CommitTest", saved.Name);
    }

    [Fact]
    public async Task RollbackAsync_ShouldNotPersistChanges()
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        var category = new Category { Name = "RollbackTest" };
        await unitOfWork.Categories.AddAsync(category);

        await transaction.RollbackAsync();

        var saved = await unitOfWork.Categories.GetAsync(category.Id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task Transaction_DisposeAsync_ShouldRollbackChanges()
    {
        var category = new Category { Name = "RollbackTest" };
        await using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            await unitOfWork.Categories.AddAsync(category);
        }

        var saved = await unitOfWork.Categories.GetAsync(category.Id);
        Assert.Null(saved);
    }
}
