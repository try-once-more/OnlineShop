using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.API.IntegrationTests;

public class CatalogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "IntegrationTests";
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetCurrentDirectory();
            config.AddJsonFile(Path.Combine(projectDir, "appsettings.json"), optional: false)
                  .AddJsonFile(Path.Combine(projectDir, $"appsettings.{environment}.json"), optional: true)
                  .AddEnvironmentVariables();
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthenticationHandler.TestScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddAuthentication(TestAuthenticationHandler.TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.TestScheme, options => { });
        });
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Database reset only")]
    internal async ValueTask ResetDatabaseAsync()
    {
        var context = Services.GetRequiredService<DbContext>();

        var schema = context.Model.GetDefaultSchema();
        schema = string.IsNullOrWhiteSpace(schema)
            ? string.Empty
            : $"[{schema}].";
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}[Products]");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}[Categories]");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}[Events]");
    }
}
