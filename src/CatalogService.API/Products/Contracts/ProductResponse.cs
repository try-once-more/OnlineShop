using CatalogService.API.Common;

namespace CatalogService.API.Products.Contracts;

/// <summary>
/// Response model representing a product.
/// </summary>
/// <param name="Id">The unique identifier of the product.</param>
/// <param name="Name">The name of the product.</param>
/// <param name="CategoryId">The identifier of the product's category.</param>
/// <param name="Price">The price of the product.</param>
/// <param name="Amount">The amount of the product in stock.</param>
/// <param name="ImageUrl">The URL of the image representing the product.</param>
/// <param name="Description">The description of the product.</param>
public record ProductResponse(
    int Id,
    string Name,
    int CategoryId,
    decimal Price,
    int Amount,
    string? ImageUrl = null,
    string? Description = null
)
{
    /// <summary>
    /// Gets or sets the HATEOAS links associated with the product.
    /// </summary>
    public IDictionary<string, Link>? Links { get; set; }
}
