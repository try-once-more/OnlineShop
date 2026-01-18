namespace CatalogService.Application.Abstractions.Repository;

//Exposing IQueryable breaks abstraction but needed for GraphQL performance.
public interface IQueryableRepository<T>
{
    IQueryable<T> AsQueryable();
}
