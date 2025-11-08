using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to delete a category.
/// </summary>
public record DeleteCategoryCommand : IRequest
{
    /// <summary>
    /// ID of the category to delete.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }

    /// <summary>
    /// If true, deletes the category along with all its child categories and associated products.
    /// </summary>
    public bool ForceDelete { get; init; } = false;
}

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

        if (request.ForceDelete)
        {
            await ForceDeleteCategoryInternalAsync(request.Id, cancellationToken);
            return;
        }

        await DeleteCategoryInternalAsync(request.Id, cancellationToken);
    }

    private async Task ForceDeleteCategoryInternalAsync(int id, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var categoriesToDelete = await unitOfWork.Categories.ListAsync(
                new QueryOptions<Category> { Filter = c => c.Id == id || c.ParentCategory.Id == id }, cancellationToken);

        var categoryIds = categoriesToDelete.Select(c => c.Id).ToHashSet();

        logger?.LogInformation("Deleting categories: {CategoryIds}", categoryIds);
        var productsToDelete = await unitOfWork.Products.ListAsync(
            new QueryOptions<Product> { Filter = p => categoryIds.Contains(p.Category.Id) }, cancellationToken);

        if (productsToDelete.Any())
        {
            var productIds = productsToDelete.Select(p => p.Id);
            logger?.LogInformation("Deleting products: {ProductIds}", productIds);
            await unitOfWork.Products.DeleteRangeAsync(productIds, cancellationToken);
        }

        await unitOfWork.Categories.DeleteRangeAsync(categoryIds, cancellationToken);
        logger?.LogInformation("Deleted category {CategoryId} along with its subcategories and products.", id);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task DeleteCategoryInternalAsync(int id, CancellationToken cancellationToken)
    {
        var hasChildCategories = await unitOfWork.Categories.ExistsAsync(c => c.ParentCategory.Id == id, cancellationToken);
        if (hasChildCategories)
        {
            logger?.LogWarning("Cannot delete category {CategoryId} because it has child categories", id);
            throw new CategoryHasChildCategoriesException(id);
        }

        var hasProducts = await unitOfWork.Products.ExistsAsync(p => p.Category.Id == id, cancellationToken);
        if (hasProducts)
        {
            logger?.LogWarning("Cannot delete category {CategoryId} because it has associated products", id);
            throw new CategoryHasProductsException(id);
        }

        await unitOfWork.Categories.DeleteAsync(id, cancellationToken);

        logger?.LogInformation("Successfully deleted category with ID: {CategoryId}", id);
    }
}
