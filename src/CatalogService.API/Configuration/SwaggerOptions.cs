namespace CatalogService.API.Configuration;

internal sealed class SwaggerOptions
{
    public string ClientId { get; init; } = string.Empty;
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string TokenUrl { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = ["openid", "profile"];
}
