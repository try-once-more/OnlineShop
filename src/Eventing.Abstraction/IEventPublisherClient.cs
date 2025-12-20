namespace Eventing.Abstraction;

public interface IEventPublisherClient
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent;
}
