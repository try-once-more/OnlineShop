namespace CartService.Application.Abstractions;

public interface IEventProcessingService
{
    Task StartListeningAsync(CancellationToken cancellationToken = default);
    Task StopListeningAsync(CancellationToken cancellationToken = default);
    Task ProcessPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default);
}