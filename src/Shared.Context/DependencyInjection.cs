using Shared.Context.Correlation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddContext(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationProvider, AsyncLocalCorrelationProvider>();
        return services;
    }
}
