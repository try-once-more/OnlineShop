using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to retrieve all categories.
/// </summary>
public record GetAllCategoriesQuery : IRequest<IReadOnlyCollection<Category>>;

internal class GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllCategoriesQueryHandler>? logger = default)
    : IRequestHandler<GetAllCategoriesQuery, IReadOnlyCollection<Category>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Category>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting all categories");

        var categories = await unitOfWork.Categories.ListAsync(cancellationToken: cancellationToken);

        logger?.LogDebug("Retrieved {Count} categories", categories.Count);

        return categories;
    }
}
