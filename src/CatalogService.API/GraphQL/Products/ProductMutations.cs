using System.Diagnostics.CodeAnalysis;
using CatalogService.API.Configuration;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using HotChocolate.Authorization;
using MediatR;

namespace CatalogService.API.GraphQL.Products;

/// <summary>
/// GraphQL Mutation type for Product operations.
/// </summary>
[MutationType]
public class ProductMutations
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation data.</param>
    /// <returns>The created product.</returns>
    [Authorize(Policy = nameof(Permissions.Create))]
    [Error<CategoryNotFoundException>]
    public async Task<Product> AddProductAsync(
        AddProductCommand request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="request">The product update data.</param>
    /// <returns>True if the update was successful.</returns>
    [Authorize(Policy = nameof(Permissions.Update))]
    [Error<ProductNotFoundException>]
    [Error<CategoryNotFoundException>]
    public async Task<bool> UpdateProductAsync(
        [NotNull] UpdateProductRequest request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand
        {
            Id = request.Id,
            Name = request.Name is not null
                ? new Application.Common.Optional<string>(request.Name)
                : default,
            Description = request.Description is not null
                ? new Application.Common.Optional<string?>(request.Description)
                : default,
            ImageUrl = request.ImageUrl is not null
                ? new Application.Common.Optional<Uri?>(string.IsNullOrWhiteSpace(request.ImageUrl) ? null : new Uri(request.ImageUrl))
                : default,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Amount = request.Amount
        };

        await mediator.Send(command, cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="request">The product deletion command.</param>
    /// <returns>True if the deletion was successful.</returns>
    [Authorize(Policy = nameof(Permissions.Delete))]
    public async Task<bool> DeleteProductAsync(
        DeleteProductCommand request,
        [NotNull, Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await mediator.Send(request, cancellationToken);
        return true;
    }
}

