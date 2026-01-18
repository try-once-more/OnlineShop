using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Products;

/// <summary>
/// Used only for GraphQL performance, do not use elsewhere.
/// </summary>
public record GetQueryableProductsQuery : IRequest<IQueryable<Product>>;

internal class GetQueryableProductsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetQueryableProductsQueryHandler>? logger = default)
    : IRequestHandler<GetQueryableProductsQuery, IQueryable<Product>>
{
    public Task<IQueryable<Product>> Handle(GetQueryableProductsQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Returning IQueryable for products.");

        return Task.FromResult(unitOfWork.Products.AsQueryable());
    }
}
