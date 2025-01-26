using DbLocator.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

internal class DbContextFactory
{
    public static IDbContextFactory<DbLocatorContext> CreateDbContextFactory(
        string connectionString
    )
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbLocatorContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PooledDbContextFactory<DbLocatorContext>(optionsBuilder.Options);
    }
}
