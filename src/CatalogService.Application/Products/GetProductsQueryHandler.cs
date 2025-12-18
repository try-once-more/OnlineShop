using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to retrieve products with optional pagination.
/// </summary>
public record GetProductsQuery : IRequest<IReadOnlyCollection<Product>>
{
    /// <summary>
    /// Page number to retrieve.
    /// </summary>
    /// <remarks>
    /// Defaults to 1 if not specified.
    /// </remarks>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be positive.")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    /// <remarks>
    /// If not specified, all products are returned.
    /// </remarks>
    [Range(1, int.MaxValue, ErrorMessage = "Page size must be positive.")]
    public int? PageSize { get; init; }
}

internal class GetProductsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetProductsQueryHandler>? logger = default)
    : IRequestHandler<GetProductsQuery, IReadOnlyCollection<Product>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting products");

        QueryOptions<Product>? options = request.PageSize.HasValue
            ? new()
            {
                Take = request.PageSize,
                Skip = (request.PageNumber - 1) * request.PageSize.Value,
            } : default;

        var products = await unitOfWork.Products.ListAsync(options, cancellationToken: cancellationToken);

        logger?.LogDebug("Retrieved {Count} products", products.Count);

        return products;
    }
}
