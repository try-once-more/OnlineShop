namespace Eventing.Abstraction;

public interface IEventSubscriberClient
{
    void RegisterHandler<T>(IEventHandler<T> handler) where T : BaseEvent;
    void UnregisterHandler<T>() where T : BaseEvent;
    Task StartProcessingAsync(CancellationToken cancellationToken = default);
    Task StopProcessingAsync(CancellationToken cancellationToken = default);
}
