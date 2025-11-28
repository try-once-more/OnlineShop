using CatalogService.Application.Abstractions;
using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using Eventing.Abstraction;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Events;

internal sealed class EventPublisherService(
    IUnitOfWork unitOfWork,
    IEventPublisherFactory eventPublisherFactory,
    CatalogEventingService eventingService,
    ILogger<EventPublisherService>? logger = default)
    : IEventPublisherService
{
    public async Task PublishPendingAsync(CancellationToken cancellationToken = default)
    {
        if (!eventingService.IsEnabled)
        {
            logger?.LogInformation("Eventing is disabled, skipping publishing.");
            return;
        }

        var eventsToPublish = await unitOfWork.Events.ListAsync(
                new QueryOptions<Event> { Filter = e => !e.Processed }, cancellationToken);

        if (eventsToPublish.Count == 0)
        {
            logger?.LogInformation("No pending events to publish.");
            return;
        }

        var publisherClient = eventPublisherFactory.GetClient(eventingService.TopicName);
        foreach (var eventToPublish in eventsToPublish)
        {
            logger?.LogDebug("Publishing event {Id} of type {EventType}", eventToPublish.Id, eventToPublish.EventType);
            try
            {
                eventToPublish.ProcessedAtUtc = DateTime.UtcNow;
                eventToPublish.Processed = true;
                eventToPublish.Error = string.Empty;
                var @event = eventingService.FromEventEntity(eventToPublish);

                await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Events.UpdateAsync(eventToPublish, cancellationToken);
                await publisherClient.PublishAsync(@event, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                logger?.LogInformation("Successfully published event {Id} of type {EventType}", eventToPublish.Id, eventToPublish.EventType);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to publish event {Id} of type {EventType}", eventToPublish.Id, eventToPublish.EventType);
                try
                {
                    eventToPublish.ProcessedAtUtc = null;
                    eventToPublish.Processed = false;
                    eventToPublish.Error = ex.Message;
                    await unitOfWork.Events.UpdateAsync(eventToPublish, cancellationToken);
                }
                catch (Exception updateEx)
                {
                    logger?.LogError(updateEx, "Failed to write error status for event {Id}", eventToPublish.Id);
                }
            }
        }
    }
}
