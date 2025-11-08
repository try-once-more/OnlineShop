using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Common;
using CatalogService.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to update an existing product.
/// </summary>
public record UpdateProductCommand : IValidatableObject, IRequest
{
    /// <summary>
    /// ID of the product to update.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }

    /// <summary>
    /// Optional new name for the product.
    /// </summary>
    public Optional<string> Name { get; init; }

    /// <summary>
    /// Optional new description for the product.
    /// </summary>
    public Optional<string?> Description { get; init; }

    /// <summary>
    /// Optional new URL for the product image.
    /// </summary>
    public Optional<Uri?> ImageUrl { get; init; }

    /// <summary>
    /// Optional new category ID for the product.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be positive.")]
    public int? CategoryId { get; init; }

    /// <summary>
    /// Optional new price for the product.
    /// </summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be positive.")]
    public decimal? Price { get; init; }

    /// <summary>
    /// Optional new stock amount.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be positive.")]
    public int? Amount { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name.HasValue && (string.IsNullOrWhiteSpace(Name.Value) || Name.Value.Length > 50))
            yield return new ValidationResult("Name is required and cannot exceed 50 characters.", [nameof(Name)]);
    }
}

internal class UpdateProductCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateProductCommandHandler>? logger = default)
    : IRequestHandler<UpdateProductCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Updating product with ID: {ProductId}", request.Id);

        var existing = await unitOfWork.Products.GetAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        if (request.Name.HasValue)
            existing.Name = request.Name.Value;

        if (request.Description.HasValue)
            existing.Description = request.Description.Value;

        if (request.ImageUrl.HasValue)
            existing.ImageUrl = request.ImageUrl.Value;

        if (request.Price.HasValue)
            existing.Price = request.Price.Value;

        if (request.Amount.HasValue)
            existing.Amount = request.Amount.Value;

        if (request.CategoryId.HasValue)
        {
            existing.Category = await unitOfWork.Categories.GetAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new CategoryNotFoundException(request.CategoryId.Value);
        }

        await unitOfWork.Products.UpdateAsync(existing, cancellationToken);

        logger?.LogInformation("Successfully updated product with ID: {ProductId}", request.Id);
    }
}
