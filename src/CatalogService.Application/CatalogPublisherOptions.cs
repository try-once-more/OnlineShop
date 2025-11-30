namespace CatalogService.Application;

public record CatalogPublisherOptions
{
    internal bool IsEnabled => !string.IsNullOrWhiteSpace(TopicName);
    public required string TopicName { get; init; }
};