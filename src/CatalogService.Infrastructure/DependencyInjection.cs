using CatalogService.Application.Abstractions.Repository;
using CatalogService.Infrastructure.Persistence.Data;
using CatalogService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServiceInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
