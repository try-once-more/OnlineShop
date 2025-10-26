using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CatalogService.Infrastructure.Persistence.Repositories;

internal abstract class Repository<TEntity, TKey>(CatalogDbContext context) : IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : IEquatable<TKey>
{
    protected readonly CatalogDbContext Context = context ?? throw new ArgumentNullException(nameof(context));
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();
    protected readonly int BatchSize = 1000;

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await DbSet.AsNoTracking().AnyAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<IReadOnlyCollection<TEntity>> ListAsync(QueryOptions<TEntity>? options = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (!options.HasValue)
            return await query.AsNoTracking().ToListAsync(cancellationToken);

        if (options.Value.Filter != null)
            query = query.AsNoTracking().Where(options.Value.Filter);

        if (options.Value.OrderBy != null)
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;
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

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        await DbSet.AddRangeAsync(entities, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        DbSet.UpdateRange(entities);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        await DbSet.Where(e => e.Id.Equals(id)).ExecuteDeleteAsync(cancellationToken);

        var tracked = Context
            .ChangeTracker
            .Entries<TEntity>()
            .Where(e => e.Entity.Id.Equals(id));

        foreach (var entry in tracked)
            entry.State = EntityState.Detached;
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));
        foreach (var chunk in ids.Chunk(BatchSize))
        {
            await DbSet
                .Where(e => chunk.Contains(e.Id))
                .ExecuteDeleteAsync(cancellationToken);

            var tracked = Context.ChangeTracker.Entries<TEntity>()
                .Where(e => chunk.Contains(e.Entity.Id));

            foreach (var entry in tracked)
                entry.State = EntityState.Detached;
        }
    }
}
