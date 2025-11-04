using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to retrieve all products for a specific category.
/// </summary>
public record GetProductsByCategoryIdQuery : IRequest<IReadOnlyCollection<Product>>
{
    /// <summary>
    /// ID of the category whose products are being requested. Must be greater than 0.
    /// </summary>
    [Required, Range(1, int.MaxValue)]
    public required int CategoryId { get; init; }
}

internal class GetProductsByCategoryIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetProductsByCategoryIdQueryHandler>? logger = default)
    : IRequestHandler<GetProductsByCategoryIdQuery, IReadOnlyCollection<Product>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Product>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting products for category ID: {CategoryId}", request.CategoryId);

        var options = new QueryOptions<Product>
        {
            Filter = p => p.Category.Id == request.CategoryId
        };

        var products = await unitOfWork.Products.ListAsync(options, cancellationToken)
            ?? throw new CategoryNotFoundException(request.CategoryId);

        logger?.LogDebug("Retrieved {Count} products for category ID: {CategoryId}",
            products.Count, request.CategoryId);

        return products;
    }
}
