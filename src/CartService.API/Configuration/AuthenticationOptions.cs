namespace CartService.API.Configuration;

internal sealed class AuthenticationOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public string[] ValidIssuers { get; init; } = [];
    public string[] ValidAudiences { get; init; } = [];
    public int ClockSkewMinutes { get; init; } = 5;
    public RequiredScopeOptions[] RequiredScopes { get; init; } = [];
}

internal sealed class RequiredScopeOptions
{
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string Claim { get; init; }
    public string? Description { get; init; }
}