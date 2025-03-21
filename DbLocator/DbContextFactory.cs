using DbLocator.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DbLocator;

internal class DbContextFactory : IDesignTimeDbContextFactory<DbLocatorContext>
{
    public static IDbContextFactory<DbLocatorContext> CreateDbContextFactory(
        string connectionString
    )
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbLocatorContext>();
        optionsBuilder.UseSqlServer(connectionString);

        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
        );

        return new PooledDbContextFactory<DbLocatorContext>(optionsBuilder.Options);
    }

    public DbLocatorContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbLocatorContext>();
        optionsBuilder.UseSqlServer();

        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
        );

        return new DbLocatorContext(optionsBuilder.Options);
    }
}
