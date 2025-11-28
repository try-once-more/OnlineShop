using Eventing.Abstraction;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Eventing.Infrastructure;

internal sealed class EventDispatcher(ILogger<EventDispatcher>? logger = default)
{
    private readonly ConcurrentDictionary<Type, IEventHandlerInvoker> handlers = [];

    public void Register<T>(IEventHandler<T> handler) where T : BaseEvent
    {
        var type = typeof(T);
        if (!handlers.TryAdd(type, new EventHandlerInvoker<T>(handler)))
        {
            logger?.LogError("Handler for {Type} already registered.", type.Name);
            throw new InvalidOperationException($"Handler for {type.Name} already registered.");
        }

        logger?.LogInformation("Registered handler for {Type}", type.Name);
    }

    public void Unregister<T>() where T : BaseEvent
    {
        var type = typeof(T);
        handlers.TryRemove(type, out _);
        logger?.LogInformation("Unregistered handler for {Type}", type.Name);
    }

    public Task DispatchAsync(BaseEvent @event, CancellationToken ct)
    {
        var eventType = @event.GetType();
        if (handlers.TryGetValue(eventType, out var invoker))
        {
            return invoker.InvokeAsync(@event, ct);
        }

        logger?.LogWarning("No handler registered for {Type}", eventType.Name);
        return Task.CompletedTask;
    }

    private interface IEventHandlerInvoker
    {
        Task InvokeAsync(BaseEvent @event, CancellationToken cancellationToken);
    }

    private class EventHandlerInvoker<T>(IEventHandler<T> handler) : IEventHandlerInvoker where T : BaseEvent
    {
        public Task InvokeAsync(BaseEvent @event, CancellationToken cancellationToken)
            => handler.HandleAsync((T)@event, cancellationToken);
    }
}