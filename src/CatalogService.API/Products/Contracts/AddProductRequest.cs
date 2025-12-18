using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.Products.Contracts;

/// <summary>
/// Request model for creating a new product.
/// </summary>
/// <param name="Name">The name of the product. Cannot be empty or whitespace.</param>
/// <param name="CategoryId">The identifier of the category this product belongs to. Must be greater than 0.</param>
/// <param name="Price">The price of the product. Must be greater than 0.</param>
/// <param name="Amount">The amount of the product in stock. Must be greater than 0.</param>
/// <param name="Description">Optional description of the product.</param>
/// <param name="ImageUrl">Optional URL for the product image.</param>
public record AddProductRequest(
    [param: Required(ErrorMessage = "Name is required.")]
    [param: Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string Name,
    [param: Required(ErrorMessage = "Category ID is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Category ID must be positive.")]
    int CategoryId,
    [param: Required(ErrorMessage = "Price is required.")]
    [param: Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be positive.")]
    decimal Price,
    [param: Required(ErrorMessage = "Amount is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Amount must be positive.")]
    int Amount,
    string? Description = null,
    Uri? ImageUrl = null
);
