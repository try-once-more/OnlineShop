using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Categories;

/// <summary>
/// Used only for GraphQL performance, do not use elsewhere.
/// </summary>
public record GetQueryableCategoriesQuery : IRequest<IQueryable<Category>>;

internal class GetQueryableCategoriesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCategoriesQueryHandler>? logger = default)
    : IRequestHandler<GetQueryableCategoriesQuery, IQueryable<Category>>
{
    public Task<IQueryable<Category>> Handle(GetQueryableCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Returning IQueryable for categories.");

        return Task.FromResult(unitOfWork.Categories.AsQueryable());
    }
}
