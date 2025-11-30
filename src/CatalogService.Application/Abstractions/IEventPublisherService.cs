namespace CatalogService.Application.Abstractions;

public interface IEventPublisherService
{
    Task PublishPendingAsync(CancellationToken cancellationToken = default);
}
