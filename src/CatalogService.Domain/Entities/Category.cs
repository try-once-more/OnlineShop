using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Entities
{
    public class Category : BaseEntity<int>
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        /// <exception cref="CategoryValidationException">
        /// Thrown if the value is null, whitespace, or exceeds 50 characters.
        /// </exception>
        public required string Name
        {
            get;
            set
            {
                value = value?.Trim();
                if (string.IsNullOrWhiteSpace(value) || value.Length > 50)
                    throw new CategoryValidationException("Category name is required and cannot exceed 50 characters.");

                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the link to an image representing the category.
        /// </summary>
        public Uri? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the parent category.
        /// </summary>
        /// <remarks>
        /// A category cannot be its own parent or an ancestor of itself.
        /// The hierarchy supports up to 2 levels only.
        /// </remarks>
        /// <exception cref="CategoryValidationException">
        /// Thrown if assigning a parent would create a circular reference or exceed the allowed hierarchy depth.
        /// </exception>
        public Category? ParentCategory
        {
            get;
            set
            {
                int level = 0;
                for (var ancestor = value; ancestor != null; ancestor = ancestor.ParentCategory)
                {
                    if (ancestor == this)
                        throw new CategoryValidationException("Category cannot be its own parent or a parent of its descendants.");
                    if (++level > 2)
                        throw new CategoryValidationException("Category hierarchy supports up to 2 levels only.");
                }

                field = value;
            }
        }
    }
}
