using System.Diagnostics.CodeAnalysis;
using CatalogService.API.Configuration;
using CatalogService.Application.Categories;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using HotChocolate.Authorization;
using MediatR;

namespace CatalogService.API.GraphQL.Categories;

/// <summary>
/// GraphQL Mutation type for Category operations.
/// </summary>
[MutationType]
public class CategoryMutations
{
    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The category creation data.</param>
    /// <returns>The created category.</returns>
    [Authorize(Policy = nameof(Permissions.Create))]
    [Error<CategoryNotFoundException>]
    public async Task<Category> AddCategoryAsync(
        AddCategoryCommand request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="request">The category update data.</param>
    /// <returns>True if the update was successful.</returns>
    [Authorize(Policy = nameof(Permissions.Update))]
    [Error<CategoryNotFoundException>]
    public async Task<bool> UpdateCategoryAsync(
        [NotNull] UpdateCategoryCommand request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new Application.Categories.UpdateCategoryCommand
        {
            Id = request.Id,
            Name = request.Name is not null
                ? new Application.Common.Optional<string>(request.Name)
                : default,
            ImageUrl = request.ImageUrl is not null
                ? new Application.Common.Optional<Uri?>(string.IsNullOrWhiteSpace(request.ImageUrl) ? null : new Uri(request.ImageUrl))
                : default,
            ParentCategoryId = request.ParentCategoryId
        };

        await mediator.Send(command, cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes a category by its ID.
    /// </summary>
    /// <param name="request">The category deletion command.</param>
    /// <returns>True if the deletion was successful.</returns>
    [Authorize(Policy = nameof(Permissions.Delete))]
    [Error<CategoryHasChildCategoriesException>]
    [Error<CategoryHasProductsException>]
    public async Task<bool> DeleteCategoryAsync(
        DeleteCategoryCommand request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
        return true;
    }
}

