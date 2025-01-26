using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers
{
    internal class DatabaseServerRepository(DbContext DbLocatorDb) : IDatabaseServerRepository
    {
        public async Task<int> AddDatabaseServer(
            string databaseServerName,
            string databaseServerIpAddress
        )
        {
            var databaseServer = new DatabaseServerEntity
            {
                DatabaseServerName = databaseServerName,
                DatabaseServerIpaddress = databaseServerIpAddress,
            };

            DbLocatorDb.Add(databaseServer);
            await DbLocatorDb.SaveChangesAsync();

            return databaseServer.DatabaseServerId;
        }

        public async Task<DatabaseServer> GetDatabaseServer(int databaseServerId)
        {
            var databaseServerEntity = await DbLocatorDb
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
            var databaseServerEntities = await DbLocatorDb
                .Set<DatabaseServerEntity>()
                .ToListAsync();

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
            var databaseServer =
                await DbLocatorDb
                    .Set<DatabaseServerEntity>()
                    .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
                ?? throw new KeyNotFoundException(
                    $"Database Server with ID {databaseServerId} not found."
                );

            databaseServer.DatabaseServerName = databaseServerName;
            databaseServer.DatabaseServerIpaddress = databaseServerIpAddress;

            DbLocatorDb.Update(databaseServer);
            await DbLocatorDb.SaveChangesAsync();
        }

        public async Task DeleteDatabaseServer(int databaseServerId)
        {
            var databaseServer =
                await DbLocatorDb
                    .Set<DatabaseServerEntity>()
                    .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
                ?? throw new KeyNotFoundException(
                    $"Database Server with ID {databaseServerId} not found."
                );

            DbLocatorDb.Remove(databaseServer);
            await DbLocatorDb.SaveChangesAsync();
        }
    }
}
