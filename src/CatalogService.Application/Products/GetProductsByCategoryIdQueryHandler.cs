using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to retrieve products for a specific category with optional pagination.
/// </summary>
public record GetProductsByCategoryIdQuery : GetProductsQuery
{
    /// <summary>
    /// ID of the category whose products are being requested. Must be greater than 0.
    /// </summary>
    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be positive.")]
    public required int CategoryId { get; init; }
}

internal class GetProductsByCategoryIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetProductsByCategoryIdQueryHandler>? logger = default)
    : IRequestHandler<GetProductsByCategoryIdQuery, IReadOnlyCollection<Product>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Product>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting products for category ID: {CategoryId}", request.CategoryId);

        int? Skip = default;
        int? Take = default;
        if (request.PageSize.HasValue)
        {
            Take = request.PageSize;
            Skip = (request.PageNumber - 1) * request.PageSize.Value;
        }

        var options = new QueryOptions<Product>
        {
            Filter = p => p.Category.Id == request.CategoryId,
            Skip = Skip,
            Take = Take
        };

        var products = await unitOfWork.Products.ListAsync(options, cancellationToken)
            ?? throw new CategoryNotFoundException(request.CategoryId);

        logger?.LogDebug("Retrieved {Count} products for category ID: {CategoryId}",
            products.Count, request.CategoryId);

        return products;
    }
}
