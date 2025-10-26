using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Entities
{
    public class Category : BaseEntity<int>
    {
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

        public Uri? ImageUrl { get; set; }

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
