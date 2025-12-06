using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.Categories.Contracts;

/// <summary>
/// Request model for creating a new category.
/// </summary>
/// <param name="Name">The name of the category. Cannot be empty or whitespace.</param>
/// <param name="ImageUrl">Optional URL for the category image.</param>
/// <param name="ParentCategoryId">Optional identifier of the parent category.</param>
public record AddCategoryRequest(
    [param: Required(ErrorMessage = "Name is required.")]
    [param: Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string Name,
    Uri? ImageUrl = null,
    [param: Range(1, int.MaxValue, ErrorMessage = "Parent category ID must be positive.")]
    int? ParentCategoryId = null
);