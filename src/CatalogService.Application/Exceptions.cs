using CatalogService.Domain.Exceptions;

namespace CatalogService.Application.Exceptions;

public class CategoryNotFoundException(int id)
    : CatalogException($"Category with ID {id} not found.");

public class ProductNotFoundException(int id)
    : CatalogException($"Product with ID {id} not found.");

public class CategoryHasProductsException(int categoryId)
    : CatalogException($"Category with ID {categoryId} has associated products.");

public class CategoryHasChildCategoriesException(int categoryId)
    : CatalogException($"Category with ID {categoryId} has child categories.");
