using CatalogService.Application.Abstractions.Repository;
using CatalogService.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Infrastructure.Persistence.Repositories;

internal class UnitOfWork(CatalogDbContext context, ICategoryRepository categories, IProductRepository products) : IUnitOfWork
{
    private readonly CatalogDbContext context = context ?? throw new ArgumentNullException(nameof(context));
    private IDbContextTransaction? transaction;
    private bool disposed;

    public ICategoryRepository Categories { get; private set; } = categories ?? throw new ArgumentNullException(nameof(categories));

    public IProductRepository Products { get; private set; } = products ?? throw new ArgumentNullException(nameof(products));

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction != null)
            throw new InvalidOperationException("Transaction already started.");

        transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync(cancellationToken);
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            transaction?.Dispose();
            context.Dispose();
        }

        disposed = true;
    }
}
