using CartService.Application.Abstractions;
using CartService.Application.Services;
using CartService.Infrastructure;
using LiteDB;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCartServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartAppService>();
        return services;
    }

    public static IServiceCollection AddCartServiceInfrastructure(this IServiceCollection services, string dbPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dbPath);
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        services.AddSingleton<ILiteDatabase>(new LiteDatabase(dbPath));
        services.AddScoped<ICartRepository, LiteCartRepository>();
        return services;
    }
}