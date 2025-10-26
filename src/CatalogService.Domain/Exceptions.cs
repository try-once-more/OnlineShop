namespace CatalogService.Domain.Exceptions;

public abstract class CatalogException(string message) : Exception(message);

public class CategoryValidationException(string message) : CatalogException(message);

public class ProductValidationException(string message) : CatalogException(message);
