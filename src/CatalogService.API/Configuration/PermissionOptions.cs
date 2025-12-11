namespace CatalogService.API.Configuration;

internal sealed class PermissionOptions
{
    public required string ReadRole { get; init; }
    public required string CreateRole { get; init; }
    public required string UpdateRole { get; init; }
    public required string DeleteRole { get; init; }
}