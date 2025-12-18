using System.ComponentModel.DataAnnotations;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Common;
using CatalogService.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Categories;

/// <summary>
/// Represents a request to update an existing category.
/// </summary>
public record UpdateCategoryCommand : IValidatableObject, IRequest
{
    /// <summary>
    /// ID of the category to update.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }

    /// <summary>
    /// Optional new name for the category.
    /// </summary>
    public Optional<string> Name { get; init; }

    /// <summary>
    /// Optional URL for the category image.
    /// </summary>
    public Optional<Uri?> ImageUrl { get; init; }

    /// <summary>
    /// Optional parent category ID.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Parent category ID must be positive.")]
    public int? ParentCategoryId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name.HasValue && (string.IsNullOrWhiteSpace(Name.Value) || Name.Value.Length > 50))
            yield return new ValidationResult("Name is required and cannot exceed 50 characters.", [nameof(Name)]);
    }
}


internal class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCategoryCommandHandler>? logger = default)
    : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Updating category with ID: {CategoryId}", request.Id);

        var existing = await unitOfWork.Categories.GetAsync(request.Id, cancellationToken)
            ?? throw new CategoryNotFoundException(request.Id);

        if (request.Name.HasValue)
            existing.Name = request.Name.Value;

        if (request.ImageUrl.HasValue)
            existing.ImageUrl = request.ImageUrl.Value;

        if (request.ParentCategoryId.HasValue)
            existing.ParentCategory = await unitOfWork.Categories.GetAsync(request.ParentCategoryId.Value, cancellationToken)
                    ?? throw new CategoryNotFoundException(request.ParentCategoryId.Value);

        await unitOfWork.Categories.UpdateAsync(existing, cancellationToken);

        logger?.LogInformation("Successfully updated category with ID: {CategoryId}", request.Id);
    }
}
