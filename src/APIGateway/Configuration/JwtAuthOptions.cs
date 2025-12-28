namespace APIGateway.Configuration;

internal sealed class JwtAuthOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public string[] ValidIssuers { get; init; } = [];
    public string[] ValidAudiences { get; init; } = [];
    public int ClockSkewMinutes { get; init; } = 5;
}
