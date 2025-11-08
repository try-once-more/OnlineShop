using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to retrieve a category by ID.
/// </summary>
public record GetCategoryByIdQuery : IRequest<Category?>
{
    /// <summary>
    /// ID of the category to retrieve.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }
}

internal class GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCategoryByIdQueryHandler>? logger = default)
    : IRequestHandler<GetCategoryByIdQuery, Category?>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Getting category by ID: {CategoryId}", request.Id);

        var category = await unitOfWork.Categories.GetAsync(request.Id, cancellationToken);

        if (category is null)
        {
            logger?.LogDebug("Category with ID: {CategoryId} not found", request.Id);
        }

        return category;
    }
}
