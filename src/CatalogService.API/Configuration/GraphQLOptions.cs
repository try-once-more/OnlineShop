namespace CatalogService.API.Configuration;

public class GraphQLOptions
{
    public int MaxAllowedExecutionDepth { get; set; } = 5;
    public int ExecutionTimeoutSeconds { get; set; } = 30;
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 999;
    public bool IncludeTotalCount { get; set; }
}
