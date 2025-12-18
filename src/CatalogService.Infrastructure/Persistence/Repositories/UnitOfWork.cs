using CatalogService.Application.Abstractions.Repository;
using CatalogService.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Infrastructure.Persistence.Repositories;

internal class UnitOfWork(CatalogDbContext context, ICategoryRepository categories, IProductRepository products, IEventRepository events) : IUnitOfWork
{
    private readonly CatalogDbContext context = context ?? throw new ArgumentNullException(nameof(context));

    public ICategoryRepository Categories { get; private set; } = categories ?? throw new ArgumentNullException(nameof(categories));

    public IProductRepository Products { get; private set; } = products ?? throw new ArgumentNullException(nameof(products));

    public IEventRepository Events { get; private set; } = events ?? throw new ArgumentNullException(nameof(events));

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new Transaction(transaction);
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }

    private class Transaction(IDbContextTransaction transaction) : ITransaction
    {
        private bool completed;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (!completed)
            {
                await transaction.CommitAsync(cancellationToken);
                completed = true;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (!completed)
            {
                await transaction.RollbackAsync(cancellationToken);
                completed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!completed)
            {
                await transaction.RollbackAsync();
                completed = true;
            }

            await transaction.DisposeAsync();
        }
    }
}
