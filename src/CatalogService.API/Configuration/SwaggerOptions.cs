namespace CatalogService.API.Configuration;

internal sealed class SwaggerOptions
{
    public required string ClientId { get; init; }
    public required string AuthorizationUrl { get; init; }
    public required string TokenUrl { get; init; }
}
