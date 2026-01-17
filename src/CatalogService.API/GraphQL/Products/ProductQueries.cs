using CatalogService.API.Configuration;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using HotChocolate.Authorization;
using MediatR;

namespace CatalogService.API.GraphQL.Products;

/// <summary>
/// GraphQL Query type for Product operations.
/// </summary>
[QueryType]
public class ProductQueries
{
    /// <summary>
    /// Retrieves a list of products with optional pagination, filtering, and sorting.
    /// </summary>
    /// <returns>A queryable collection of products.</returns>
    [Authorize(Policy = nameof(Permissions.Read))]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Product>> GetProductsAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        return await mediator.Send(new GetQueryableProductsQuery(), cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>The product if found, otherwise null.</returns>
    [Authorize(Policy = nameof(Permissions.Read))]
    [UseFirstOrDefault]
    [UseProjection]
    public async Task<IQueryable<Product>> GetProductByIdAsync(
        int id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        var queryable = await mediator.Send(new GetQueryableProductsQuery(), cancellationToken);
        return queryable.Where(x => x.Id == id);
    }
}


