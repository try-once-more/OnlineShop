using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;

namespace CatalogService.Infrastructure.Persistence.Data;

internal class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CatalogDBConnectionString")
            ?? throw new InvalidOperationException("Environment variable 'CatalogDBConnectionString' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(connectionString)
            .ReplaceService<IHistoryRepository, CustomHistoryRepository>();

        return new CatalogDbContext(optionsBuilder.Options);
    }
}


internal class CustomHistoryRepository(HistoryRepositoryDependencies dependencies)
    : SqlServerHistoryRepository(dependencies)
{
    protected override string TableSchema => CatalogDbContext.DefaultSchema;
}