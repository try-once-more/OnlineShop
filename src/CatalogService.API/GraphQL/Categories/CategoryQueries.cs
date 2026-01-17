using CatalogService.API.Configuration;
using CatalogService.Application.Categories;
using CatalogService.Domain.Entities;
using HotChocolate.Authorization;
using MediatR;

namespace CatalogService.API.GraphQL.Categories;

/// <summary>
/// GraphQL Query type for Catalog Service operations.
/// </summary>
[QueryType]
public class CategoryQueries
{
    /// <summary>
    /// Retrieves a list of categories with optional pagination, filtering, and sorting.
    /// </summary>
    /// <returns>A queryable collection of categories.</returns>
    [Authorize(Policy = nameof(Permissions.Read))]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Category>> GetCategoriesAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        return await mediator.Send(new GetQueryableCategoriesQuery(), cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    /// <param name="id">The ID of the category to retrieve.</param>
    /// <returns>The category if found, otherwise null.</returns>
    [Authorize(Policy = nameof(Permissions.Read))]
    [UseFirstOrDefault]
    [UseProjection]
    public async Task<IQueryable<Category>> GetCategoryByIdAsync(
        int id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        var queryable = await mediator.Send(new GetQueryableCategoriesQuery(), cancellationToken);
        return queryable.Where(x => x.Id == id);
    }
}
