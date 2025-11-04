using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to retrieve all products.
/// </summary>
public record GetAllProductsQuery : IRequest<IReadOnlyCollection<Product>>;

internal class GetAllProductsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllProductsQueryHandler>? logger = default)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyCollection<Product>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting all products");

        var products = await unitOfWork.Products.ListAsync(cancellationToken: cancellationToken);

        logger?.LogDebug("Retrieved {Count} products", products.Count);

        return products;
    }
}
