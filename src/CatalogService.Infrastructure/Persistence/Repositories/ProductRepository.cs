using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace CatalogService.Infrastructure.Persistence.Repositories;

internal class ProductRepository(CatalogDbContext context)
    : Repository<Product, int>(context), IProductRepository
{
    public async override Task<Product?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .ThenInclude(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async override Task<Product?> GetAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .ThenInclude(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async override Task<IReadOnlyCollection<Product>> ListAsync(QueryOptions<Product>? options = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = DbSet
            .Include(e => e.Category)
            .ThenInclude(c => c.ParentCategory);

        if (!options.HasValue)
            return await query.AsNoTracking().ToListAsync(cancellationToken);

        if (options.Value.Filter != null)
            query = query.AsNoTracking().Where(options.Value.Filter);

        if (options.Value.OrderBy != null)
        {
            IOrderedQueryable<Product>? orderedQuery = null;
            foreach (var sort in options.Value.OrderBy)
            {
                orderedQuery = sort.Apply(query, orderedQuery);
            }
            query = orderedQuery ?? query;
        }

        if (options.Value.Skip.HasValue)
            query = query.Skip(options.Value.Skip.Value);

        if (options.Value.Take.HasValue)
            query = query.Take(options.Value.Take.Value);

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }
}
