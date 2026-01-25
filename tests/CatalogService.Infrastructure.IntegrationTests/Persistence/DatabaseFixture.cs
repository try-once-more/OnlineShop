using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.IntegrationTests.Persistence;

public class DatabaseFixture : IAsyncLifetime
{
    public ServiceProvider ServiceProvider { get; private set; }

    public DatabaseFixture()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "IntegrationTests";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.Configure<CatalogDatabaseOptions>(configuration.GetSection("ConnectionStrings"));
        services.AddCatalogServiceInfrastructure();
        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        var context = ServiceProvider.GetRequiredService<DbContext>();
        if (!await context.Database.CanConnectAsync())
        {
            throw new InvalidOperationException("Failed to connect to the IntegrationTests database.");
        }
    }

    public Task DisposeAsync() => ServiceProvider.DisposeAsync().AsTask();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Database reset only")]
    internal static async Task ResetDatabaseAsync(DbContext context)
    {
        var schema = context.Model.GetDefaultSchema();
        schema = string.IsNullOrWhiteSpace(schema)
            ? string.Empty
            : $"[{schema}].";
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {schema}[Products]");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}[Categories]");
    }
}
