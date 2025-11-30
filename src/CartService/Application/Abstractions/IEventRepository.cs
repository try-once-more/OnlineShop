using Eventing.Abstraction;

namespace CartService.Application.Abstractions;

internal interface IEventRepository
{
    Task SaveEventAsync(BaseEvent @event, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BaseEvent>> GetPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(BaseEvent @event, CancellationToken cancellationToken = default);
}
