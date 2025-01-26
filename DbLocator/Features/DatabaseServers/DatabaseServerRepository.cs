using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers
{
    internal class DatabaseServerRepository(IDbContextFactory<DbLocatorContext> dbContextFactory)
        : IDatabaseServerRepository
    {
        public async Task<int> AddDatabaseServer(
            string databaseServerName,
            string databaseServerIpAddress
        )
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseServer = new DatabaseServerEntity
            {
                DatabaseServerName = databaseServerName,
                DatabaseServerIpaddress = databaseServerIpAddress,
            };

            dbContext.Add(databaseServer);
            await dbContext.SaveChangesAsync();

            return databaseServer.DatabaseServerId;
        }

        public async Task<DatabaseServer> GetDatabaseServer(int databaseServerId)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseServerEntity = await dbContext
                .Set<DatabaseServerEntity>()
                .Where(ds => ds.DatabaseServerId == databaseServerId)
                .FirstOrDefaultAsync();

            return new DatabaseServer(
                databaseServerEntity.DatabaseServerId,
                databaseServerEntity.DatabaseServerName,
                databaseServerEntity.DatabaseServerIpaddress
            );
        }

        public async Task<List<DatabaseServer>> GetDatabaseServers()
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseServerEntities = await dbContext.Set<DatabaseServerEntity>().ToListAsync();

            return databaseServerEntities
                .Select(ds => new DatabaseServer(
                    ds.DatabaseServerId,
                    ds.DatabaseServerName,
                    ds.DatabaseServerIpaddress
                ))
                .ToList();
        }

        public async Task UpdateDatabaseServer(
            int databaseServerId,
            string databaseServerName,
            string databaseServerIpAddress
        )
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseServer =
                await dbContext
                    .Set<DatabaseServerEntity>()
                    .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
                ?? throw new KeyNotFoundException(
                    $"Database Server with ID {databaseServerId} not found."
                );

            databaseServer.DatabaseServerName = databaseServerName;
            databaseServer.DatabaseServerIpaddress = databaseServerIpAddress;

            dbContext.Update(databaseServer);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteDatabaseServer(int databaseServerId)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseServer =
                await dbContext
                    .Set<DatabaseServerEntity>()
                    .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
                ?? throw new KeyNotFoundException(
                    $"Database Server with ID {databaseServerId} not found."
                );

            dbContext.Remove(databaseServer);
            await dbContext.SaveChangesAsync();
        }
    }
}
