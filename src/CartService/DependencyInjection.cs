using CartService.Application.Abstractions;
using CartService.Application.Handlers;
using CartService.Application.Services;
using CartService.Infrastructure;
using CatalogService.Events.Products;
using Eventing.Abstraction;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCartServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartAppService>();

        services.AddScoped<IIntegrationEventHandler<ProductUpdatedEvent>, ProductUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ProductDeletedEvent>, ProductDeletedEventHandler>();

        services.AddSingleton<BaseEvent, ProductUpdatedEvent>();
        services.AddSingleton<BaseEvent, ProductDeletedEvent>();
        services.AddSingleton<ProductUpdatedEventPersistHandler>();
        services.AddSingleton<ProductDeletedEventPersistHandler>();

        services.AddSingleton<ProductUpdatedEventPersistHandler>();
        services.AddSingleton<IEventProcessingService, EventProcessingService>();

        return services;
    }

    public static IServiceCollection AddCartServiceInfrastructure(this IServiceCollection services)
    {
        var mapper = BsonMapper.Global;
        mapper.RegisterType<Uri>(
            serialize: uri => uri?.ToString() ?? BsonValue.Null,
            deserialize: bson => bson.IsNull ? null : new Uri(bson.AsString, UriKind.RelativeOrAbsolute)
        );

        services.AddSingleton<ILiteDatabase>(provider =>
        {
            var dbSettings = provider.GetRequiredService<IOptions<CartDatabaseOptions>>().Value;
            if (string.IsNullOrWhiteSpace(dbSettings.CartDatabase))
                throw new InvalidOperationException("'CartDatabase' connection string is not configured.");

            return new LiteDatabase(dbSettings.CartDatabase);
        });
        services.AddScoped<ICartRepository, LiteCartRepository>();
        services.AddScoped<IEventRepository, LiteEventRepository>();

        return services;
    }
}
