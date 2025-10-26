using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

public record GetCategoryByIdQuery([property: Required, Range(1, int.MaxValue)] int Id) : IRequest<Category?>;

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
