using CatalogService.API.Common;

namespace CatalogService.API.Categories.Contracts;

/// <summary>
/// Response model representing a category.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="ImageUrl">The URL of the image representing the category.</param>
/// <param name="ParentCategoryId">The identifier of the parent category, if any.</param>
public record CategoryResponse(
    int Id,
    string Name,
    string? ImageUrl = null,
    int? ParentCategoryId = null
)
{
    /// <summary>
    /// Gets or sets the HATEOAS links associated with the category.
    /// </summary>
    public IDictionary<string, Link>? Links { get; set; }
}
