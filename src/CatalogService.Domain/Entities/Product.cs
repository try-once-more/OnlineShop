using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Entities
{
    public class Product : BaseEntity<int>
    {
        public required string Name
        {
            get;
            set
            {
                value = value?.Trim();
                if (string.IsNullOrWhiteSpace(value) || value.Length > 50)
                    throw new ProductValidationException("Product name is required and cannot exceed 50 characters.");

                field = value;
            }
        }

        public string? Description { get; set; }

        public Uri? ImageUrl { get; set; }

        public required Category Category
        {
            get => field;
            set => field = value ?? throw new ProductValidationException("Product must have a category assigned.");
        }

        public required decimal Price
        {
            get;
            set
            {
                if (value <= 0)
                    throw new ProductValidationException("Price must be positive.");

                field = value;
            }
        }

        public required int Amount
        {
            get;
            set
            {
                if (value <= 0)
                    throw new ProductValidationException("Amount must be positive.");

                field = value;
            }
        }
    }
}
