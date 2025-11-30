namespace CatalogService.Domain.Entities;

public class Event : BaseEntity<Guid>
{
    public required DateTime OccurredAtUtc { get; set; }
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public bool Processed { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}
