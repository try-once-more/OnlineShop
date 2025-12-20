namespace CatalogService.Infrastructure;

public record CatalogDatabaseOptions
{
    public required string CatalogDatabase { get; set; }
}
