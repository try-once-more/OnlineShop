namespace CartService.API.Configuration;

internal sealed class SwaggerOptions
{
    public string? Scheme { get; init; }
    public int? Port { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string TokenUrl { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = ["openid", "profile"];
}
