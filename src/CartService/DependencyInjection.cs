using CartService.Application.Abstractions;
using CartService.Application.Services;
using CartService.Infrastructure;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public record CartDatabaseSettings
{
    public string? CartDatabase { get; set; }
}

public static class DependencyInjection
{
    public static IServiceCollection AddCartServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartAppService>();
        return services;
    }

    public static IServiceCollection AddCartServiceInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILiteDatabase>(provider =>
        {
            var dbSettings = provider.GetRequiredService<IOptions<CartDatabaseSettings>>().Value;
            if (string.IsNullOrWhiteSpace(dbSettings?.CartDatabase))
                throw new InvalidOperationException("'CartDatabase' connection string is not configured.");

            return new LiteDatabase(dbSettings.CartDatabase);
        });
        services.AddScoped<ICartRepository, LiteCartRepository>();
        return services;
    }
}