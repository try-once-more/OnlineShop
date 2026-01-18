using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.GraphQL.Categories;

/// <summary>
/// Model for updating an existing category.
/// </summary>
/// <param name="Id">Identifier of the category to update.</param>
/// <param name="Name">Optional new name for the category. Cannot exceed 50 characters.</param>
/// <param name="ImageUrl">Optional new URL for the category image.</param>
/// <param name="ParentCategoryId">Optional new parent category identifier.</param>
public record UpdateCategoryCommand(
    [param: Required(ErrorMessage = "Id is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Id must be positive.")]
    int Id,
    [param: Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string? Name,
    string? ImageUrl,
    [param: Range(1, int.MaxValue, ErrorMessage = "Parent category ID must be positive.")]
    int? ParentCategoryId = null
);
