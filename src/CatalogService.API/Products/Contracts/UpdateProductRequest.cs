using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.Products.Contracts;

/// <summary>
/// Request model for updating an existing product.
/// </summary>
/// <param name="Name">Optional new name for the product. Cannot exceed 50 characters.</param>
/// <param name="Description">Optional new description for the product.</param>
/// <param name="ImageUrl">Optional new URL for the product image.</param>
/// <param name="CategoryId">Optional new category identifier for the product.</param>
/// <param name="Price">Optional new price for the product. Must be greater than 0.</param>
/// <param name="Amount">Optional new stock amount. Must be greater than 0.</param>
public record UpdateProductRequest(
    [param: Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string? Name,
    string? Description,
    Uri? ImageUrl,
    [param: Range(1, int.MaxValue, ErrorMessage = "Category ID must be positive.")]
    int? CategoryId,
    [param: Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be positive.")]
    decimal? Price,
    [param: Range(1, int.MaxValue, ErrorMessage = "Amount must be positive.")]
    int? Amount
);
