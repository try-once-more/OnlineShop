internal sealed class CorsOptions
{
    internal string[] AllowedOrigins { get; init; } = [];
    internal string[] AllowedMethods { get; init; } = [];
    internal string[] AllowedHeaders { get; init; } = [];
}