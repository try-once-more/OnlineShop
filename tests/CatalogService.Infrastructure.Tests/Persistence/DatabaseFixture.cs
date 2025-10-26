using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.Tests.Persistence;

public class DatabaseFixture : IAsyncLifetime
{
    public ServiceProvider ServiceProvider { get; private set; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("IntegrationTests")
            ?? throw new InvalidOperationException("Connection string 'IntegrationTests' not found");

        var services = new ServiceCollection()
            .AddCatalogServiceInfrastructure(connectionString);
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
