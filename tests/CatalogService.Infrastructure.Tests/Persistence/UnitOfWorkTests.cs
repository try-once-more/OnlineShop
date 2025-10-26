using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.Tests.Persistence;

[Collection(nameof(DatabaseFixture))]
public class UnitOfWorkTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IServiceScope scope = fixture.ServiceProvider.CreateScope();
    private IUnitOfWork unitOfWork;

    public Task InitializeAsync()
    {
        unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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

    [Fact]
    public async Task CommitAsync_ShouldPersistChanges()
    {
        await unitOfWork.BeginTransactionAsync();

        var category = new Category { Name = "CommitTest" };
        await unitOfWork.Categories.AddAsync(category);

        await unitOfWork.CommitAsync();

        var saved = await unitOfWork.Categories.GetAsync(category.Id);
        Assert.NotNull(saved);
        Assert.Equal("CommitTest", saved.Name);
    }

    [Fact]
    public async Task RollbackAsync_ShouldNotPersistChanges()
    {
        await unitOfWork.BeginTransactionAsync();

        var category = new Category { Name = "RollbackTest" };
        await unitOfWork.Categories.AddAsync(category);

        await unitOfWork.RollbackAsync();

        var saved = await unitOfWork.Categories.GetAsync(category.Id);
        Assert.Null(saved);
    }
}
