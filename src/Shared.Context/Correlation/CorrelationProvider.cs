namespace Shared.Context.Correlation;

public interface ICorrelationProvider
{
    string? CorrelationId { get; }
    void Set(string correlationId);
}

internal class AsyncLocalCorrelationProvider : ICorrelationProvider
{
    private static readonly AsyncLocal<string?> correlation = new();
    public string? CorrelationId => correlation.Value;
    public void Set(string correlationId) => correlation.Value = correlationId;
}
