namespace Eventing.Abstraction;

public interface IEventSubscriberClient
{
    void RegisterHandler<T>(IEventHandler<T> handler) where T : BaseEvent;
    void UnregisterHandler<T>() where T : BaseEvent;
    Task StartListeningAsync(CancellationToken cancellationToken = default);
    Task StopListeningAsync(CancellationToken cancellationToken = default);
}
