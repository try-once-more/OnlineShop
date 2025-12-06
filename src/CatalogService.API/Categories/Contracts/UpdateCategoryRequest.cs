using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.Categories.Contracts;

/// <summary>
/// Request model for updating an existing category.
/// </summary>
/// <param name="Name">Optional new name for the category. Cannot exceed 50 characters.</param>
/// <param name="ImageUrl">Optional new URL for the category image.</param>
/// <param name="ParentCategoryId">Optional new parent category identifier.</param>
public record UpdateCategoryRequest(
    [param: Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string? Name,
    [param: Url(ErrorMessage = "ImageUrl must be a valid URL.")]
    Uri? ImageUrl,
    [param: Range(1, int.MaxValue, ErrorMessage = "Parent category ID must be positive.")]
    int? ParentCategoryId = null
);