namespace Eventing.Abstraction;

public interface IEventHandler<in T> where T : BaseEvent
{
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}
