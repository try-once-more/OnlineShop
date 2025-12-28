namespace APIGateway.Configuration;

internal sealed class AdminRole
{
    public RequiredClaim[] RequiredClaims { get; init; } = [];
}

internal sealed class RequiredClaim
{
    public required string Type { get; init; }
    public required string Value { get; init; }
}
