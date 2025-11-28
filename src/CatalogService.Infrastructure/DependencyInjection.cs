using CatalogService.Application.Abstractions.Repository;
using CatalogService.Infrastructure.Persistence.Data;
using CatalogService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public record CatalogDatabaseSettings
{
    public required string CatalogDatabase { get; set; }
}

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServiceInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<CatalogDbContext>((provider, options) =>
        {
            var dbSettings = provider.GetRequiredService<IOptions<CatalogDatabaseSettings>>().Value;
            if (string.IsNullOrWhiteSpace(dbSettings.CatalogDatabase))
                throw new InvalidOperationException("'CatalogDatabase' connection string is not configured.");

            options.UseSqlServer(dbSettings.CatalogDatabase);
        });
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
