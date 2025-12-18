using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to retrieve categories with optional pagination.
/// </summary>
public record GetCategoriesQuery : IRequest<IReadOnlyCollection<Category>>
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
    /// If not specified, all categories are returned.
    /// </remarks>
    [Range(1, int.MaxValue, ErrorMessage = "Page size must be positive.")]
    public int? PageSize { get; init; }
}

internal class GetCategoriesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCategoriesQueryHandler>? logger = default)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<Category>>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<IReadOnlyCollection<Category>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting categories");

        QueryOptions<Category>? options = request.PageSize.HasValue
            ? new()
            {
                Take = request.PageSize,
                Skip = (request.PageNumber - 1) * request.PageSize.Value,
            } : default;

        var categories = await unitOfWork.Categories.ListAsync(options, cancellationToken: cancellationToken);

        logger?.LogDebug("Retrieved {Count} categories", categories.Count);

        return categories;
    }
}
