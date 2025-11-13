using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.API.Tests;

public class CatalogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "IntegrationTests";
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetCurrentDirectory();
            config.AddJsonFile(Path.Combine(projectDir, "appsettings.json"), optional: false)
                  .AddJsonFile(Path.Combine(projectDir, $"appsettings.{environment}.json"), optional: true)
                  .AddEnvironmentVariables();
        });
    }

    internal async Task ResetDatabaseAsync()
    {
        var context = Services.GetRequiredService<DbContext>();

        var schema = context.Model.GetDefaultSchema();
        schema = string.IsNullOrWhiteSpace(schema)
            ? string.Empty
            : $"[{schema}].";
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {schema}[Products]");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}[Categories]");
    }
}