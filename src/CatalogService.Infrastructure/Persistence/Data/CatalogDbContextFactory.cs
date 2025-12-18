using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Infrastructure.Persistence.Data;

internal class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CatalogDBConnectionString")
            ?? throw new InvalidOperationException("Environment variable 'CatalogDBConnectionString' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(connectionString, options =>
            {
                // Set the schema for the migrations history table using the public API
                options.MigrationsHistoryTable(
                    tableName: "__EFMigrationsHistory",
                    schema: CatalogDbContext.DefaultSchema
                );
            });

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
