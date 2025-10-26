using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

public record AddCategoryCommand(
    [property: Required, MaxLength(50)] string Name,
    Uri? ImageUrl,
    int? ParentCategoryId) : IRequest<Category>;

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
