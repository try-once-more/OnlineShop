using CatalogService.Domain.Entities;
using System.Linq.Expressions;

namespace CatalogService.Application.Abstractions.Repository;

public interface ICategoryRepository : IRepository<Category, int>;
public interface IProductRepository : IRepository<Product, int>;

public interface ISort<TEntity>
{
    IOrderedQueryable<TEntity> Apply(IQueryable<TEntity> query, IOrderedQueryable<TEntity>? orderedQuery = null);
}

public readonly record struct QueryOptions<TEntity>(
    Expression<Func<TEntity, bool>>? Filter = null,
    IReadOnlyList<ISort<TEntity>>? OrderBy = null,
    int? Skip = null,
    int? Take = null
);

public interface IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : IEquatable<TKey>
{
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> ListAsync(QueryOptions<TEntity>? options = null, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
}
