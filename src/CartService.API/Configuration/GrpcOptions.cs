namespace CartService.API.Configuration;

internal sealed class GrpcOptions
{
    public bool EnableDetailedErrors { get; init; }
    public int? MaxReceiveMessageSize { get; init; }
    public int? MaxSendMessageSize { get; init; }
    public bool EnableReflection { get; init; }
    public CompressionOptions? ResponseCompression { get; init; }
 
    internal sealed class CompressionOptions
    {
        public string Algorithm { get; init; } = "gzip";
        public string Level { get; init; } = "Optimal";
    }
}
