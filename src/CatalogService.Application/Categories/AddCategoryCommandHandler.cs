using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to create a new category.
/// </summary>
public record AddCategoryCommand : IRequest<Category>
{
    /// <summary>
    /// Category name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional URL for the category image.
    /// </summary>
    public Uri? ImageUrl { get; init; }

    /// <summary>
    /// Optional parent category ID.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Parent category ID must be positive.")]
    public int? ParentCategoryId { get; init; }
}


internal class AddCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<AddCategoryCommandHandler>? logger = default)
    : IRequestHandler<AddCategoryCommand, Category>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Category> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Adding category with name: {CategoryName}, ParentCategoryId: {ParentCategoryId}",
            request.Name, request.ParentCategoryId);

        Category? parentCategory = request.ParentCategoryId.HasValue
            ? await unitOfWork.Categories.GetAsync(request.ParentCategoryId.Value, cancellationToken)
                ?? throw new CategoryNotFoundException(request.ParentCategoryId.Value)
            : null;

        var category = new Category
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            ParentCategory = parentCategory
        };

        await unitOfWork.Categories.AddAsync(category, cancellationToken);

        logger?.LogInformation("Successfully added category with ID: {CategoryId}, Name: {CategoryName}",
            category.Id, category.Name);

        return category;
    }
}
