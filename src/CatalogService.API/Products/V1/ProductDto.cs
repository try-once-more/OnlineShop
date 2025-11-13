using CatalogService.API.Common;

namespace CatalogService.API.Products.V1;

/// <summary>
/// DTO for a product.
/// </summary>
public record ProductDto
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the URL of the image representing the product.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Gets the description of the product.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the identifier of the product's category.
    /// </summary>
    public required int CategoryId { get; init; }

    /// <summary>
    /// Gets the price of the product.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Gets the amount of the product in stock.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Gets a collection of HATEOAS links associated with the category.
    /// The key is the link relation, and the value is the URL.
    /// </summary>
    public IDictionary<string, Link>? Links { get; set; }
}
