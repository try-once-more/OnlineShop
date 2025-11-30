using Eventing.Abstraction;

namespace CartService.Application.Abstractions;

internal interface IIntegrationEventHandler<in TEvent> where TEvent : BaseEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
