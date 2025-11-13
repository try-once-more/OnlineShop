using CatalogService.API.Common;

namespace CatalogService.API.Categories.V1;

/// <summary>
/// DTO for a category.
/// </summary>
public record CategoryDto
{
    /// <summary>
    /// Gets the unique identifier of the category.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the category.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the URL of the image representing the category.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Gets the identifier of the parent category, if any.
    /// </summary>
    public int? ParentCategoryId { get; init; }

    /// <summary>
    /// Gets a collection of HATEOAS links associated with the category.
    /// The key is the link relation, and the value is the URL.
    /// </summary>
    public IDictionary<string, Link>? Links { get; set; }
}