using DbLocator.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DbLocator;

internal class DbContextFactory
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
}
