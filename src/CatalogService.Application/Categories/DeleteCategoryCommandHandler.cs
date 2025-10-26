using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

public record DeleteCategoryCommand([property: Required, Range(1, int.MaxValue)] int Id) : IRequest;

internal class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCategoryCommandHandler>? logger = default)
    : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Attempting to delete category with ID: {CategoryId}", request.Id);

        var category = await unitOfWork.Categories.GetAsync(request.Id, cancellationToken);
        if (category is null)
        {
            logger?.LogWarning("Category with ID: {CategoryId} not found for deletion", request.Id);
            return;
        }

        var hasChildCategories = await unitOfWork.Categories.ExistsAsync(c => c.ParentCategory.Id == request.Id, cancellationToken);
        if (hasChildCategories)
        {
            logger?.LogWarning("Cannot delete category {CategoryId} because it has child categories", request.Id);
            throw new CategoryHasChildCategoriesException(request.Id);
        }

        var hasProducts = await unitOfWork.Products.ExistsAsync(p => p.Category.Id == category.Id, cancellationToken);
        if (hasProducts)
        {
            logger?.LogWarning("Cannot delete category {CategoryId} because it has associated products", request.Id);
            throw new CategoryHasProductsException(request.Id);
        }

        await unitOfWork.Categories.DeleteAsync(request.Id, cancellationToken);

        logger?.LogInformation("Successfully deleted category with ID: {CategoryId}", request.Id);
    }
}
