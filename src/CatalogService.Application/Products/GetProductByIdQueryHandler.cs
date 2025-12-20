using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to retrieve a specific product by its ID.
/// </summary>
public record GetProductByIdQuery : IRequest<Product?>
{
    /// <summary>
    /// ID of the product to retrieve.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }
}

internal class GetProductByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetProductByIdQueryHandler>? logger = default)
    : IRequestHandler<GetProductByIdQuery, Product?>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting product by ID: {ProductId}", request.Id);

        var product = await unitOfWork.Products.GetAsync(request.Id, cancellationToken);

        if (product is null)
        {
            logger?.LogDebug("Product with ID: {ProductId} not found", request.Id);
        }

        return product;
    }
}
