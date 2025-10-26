using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

public record GetProductByIdQuery([property: Required, Range(1, int.MaxValue)] int Id) : IRequest<Product?>;

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
