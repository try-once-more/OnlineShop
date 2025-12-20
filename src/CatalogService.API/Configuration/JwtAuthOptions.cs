namespace CatalogService.API.Configuration;

public sealed class JwtAuthOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public string[] ValidIssuers { get; init; } = [];
    public string[] ValidAudiences { get; init; } = [];
    public int ClockSkewMinutes { get; init; } = 5;
    public RequiredClaim[] RequiredClaims { get; init; } = [];
    public required Permissions PermissionClaims { get; init; }
}

public sealed class Permissions
{
    public required RequiredClaim Read { get; init; }
    public required RequiredClaim Create { get; init; }
    public required RequiredClaim Update { get; init; }
    public required RequiredClaim Delete { get; init; }
}

public sealed class RequiredClaim
{
    public required string Type { get; init; }
    public required string Value { get; init; }
}
