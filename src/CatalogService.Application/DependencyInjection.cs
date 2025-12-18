using CatalogService.Application.Abstractions;
using CatalogService.Application.Events;
using CatalogService.Application.Pipeline;
using CatalogService.Events.Products;
using Eventing.Abstraction;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServiceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IEventPublisherService, EventPublisherService>();

        services.AddSingleton<BaseEvent, ProductCreatedEvent>();
        services.AddSingleton<BaseEvent, ProductDeletedEvent>();
        services.AddSingleton<BaseEvent, ProductUpdatedEvent>();

        return services;
    }
}
