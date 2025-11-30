using CatalogService.Application.Abstractions;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Eventing.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CatalogService.Application.Events;

internal sealed class EventPublisherService(
    IUnitOfWork unitOfWork,
    IEventPublisherFactory eventPublisherFactory,
    IOptions<CatalogPublisherOptions> options,
    ILogger<EventPublisherService>? logger = default)
    : IEventPublisherService
{
    public async Task PublishPendingAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.IsEnabled)
        {
            logger?.LogInformation("Eventing is disabled, skipping publishing.");
            return;
        }

        var entities = await unitOfWork.Events.ListAsync(
                new QueryOptions<Event> { Filter = e => !e.Processed }, cancellationToken);

        if (entities.Count == 0)
        {
            logger?.LogInformation("No pending events to publish.");
            return;
        }

        var publisherClient = eventPublisherFactory.GetClient(options.Value.TopicName);
        foreach (var entity in entities)
        {
            logger?.LogDebug("Publishing event {Id} of type {EventType}", entity.Id, entity.EventType);
            try
            {
                entity.ProcessedAtUtc = DateTime.UtcNow;
                entity.Processed = true;
                entity.Error = string.Empty;
                var @event = entity.ToEvent();

                await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Events.UpdateAsync(entity, cancellationToken);
                await publisherClient.PublishAsync(@event, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                logger?.LogInformation("Successfully published event {Id} of type {EventType}", entity.Id, entity.EventType);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to publish event {Id} of type {EventType}", entity.Id, entity.EventType);
                try
                {
                    entity.ProcessedAtUtc = null;
                    entity.Processed = false;
                    entity.Error = ex.Message;
                    await unitOfWork.Events.UpdateAsync(entity, cancellationToken);
                }
                catch (Exception updateEx)
                {
                    logger?.LogError(updateEx, "Failed to write error status for event {Id}", entity.Id);
                }
            }
        }
    }
}
