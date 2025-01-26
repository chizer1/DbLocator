using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases
{
    internal class DatabaseRepository(DbContext DbLocatorDb) : IDatabaseRepository
    {
        public async Task<int> AddDatabase(
            string databaseName,
            string databaseUser,
            int databaseServerId,
            byte databaseTypeId,
            Status databaseStatus,
            bool useTrustedConnection,
            bool createDatabase
        )
        {
            var database = new DatabaseEntity
            {
                DatabaseName = databaseName,
                DatabaseServerId = databaseServerId,
                DatabaseTypeId = databaseTypeId,
                DatabaseStatusId = (byte)databaseStatus,
                UseTrustedConnection = useTrustedConnection,
            };

            var commands = new List<string>();

            if (createDatabase)
                commands.Add($"CREATE DATABASE {databaseName}");

            if (!useTrustedConnection)
            {
                database.DatabaseUser = databaseUser;

                var password = Guid.NewGuid().ToString();
                database.DatabaseUserPassword = password; // encrypt here later

                commands.AddRange(
                    [
                        $"CREATE LOGIN {databaseUser} WITH PASSWORD = '{password}'",
                        $"USE {databaseName}; CREATE USER {databaseUser} FOR LOGIN {databaseUser}",
                    ]
                );
            }

            await EnsureDatabaseExists(databaseName, createDatabase);
            await EnsureUserDoesNotExist(databaseUser);

            await DbLocatorDb.Set<DatabaseEntity>().AddAsync(database);
            await DbLocatorDb.SaveChangesAsync();

            foreach (var commandText in commands)
            {
                using var command = DbLocatorDb.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await DbLocatorDb.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();

            return database.DatabaseId;
        }

        private async Task EnsureDatabaseExists(string databaseName, bool createDatabase)
        {
            using var command = DbLocatorDb.Database.GetDbConnection().CreateCommand();
            command.CommandText =
                $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
            await DbLocatorDb.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            if (result is int count)
            {
                if (createDatabase && count > 0)
                    throw new InvalidOperationException($"Database {databaseName} already exists.");
                if (!createDatabase && count == 0)
                    throw new InvalidOperationException($"Database {databaseName} not found.");
            }
        }

        private async Task EnsureUserDoesNotExist(string databaseUser)
        {
            using var command = DbLocatorDb.Database.GetDbConnection().CreateCommand();
            command.CommandText =
                $"SELECT COUNT(*) FROM sys.server_principals WHERE name = '{databaseUser}'";
            await DbLocatorDb.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            if (result is int count && count > 0)
                throw new InvalidOperationException($"User {databaseUser} already exists.");
        }

        public async Task<Database> GetDatabase(int databaseId)
        {
            var database =
                await DbLocatorDb
                    .Set<DatabaseEntity>()
                    .Include(d => d.DatabaseServer)
                    .Include(d => d.DatabaseType)
                    .FirstOrDefaultAsync(d => d.DatabaseId == databaseId)
                ?? throw new InvalidOperationException("Database not found.");

            var databaseServer = new DatabaseServer(
                database.DatabaseServer.DatabaseServerId,
                database.DatabaseServer.DatabaseServerName,
                database.DatabaseServer.DatabaseServerIpaddress
            );

            var databaseType = new DatabaseType(
                database.DatabaseType.DatabaseTypeId,
                database.DatabaseType.DatabaseTypeName
            );

            return new Database(
                database.DatabaseId,
                database.DatabaseName,
                databaseType,
                databaseServer,
                (Status)database.DatabaseStatusId,
                database.UseTrustedConnection
            );
        }

        public async Task<List<Database>> GetDatabases()
        {
            var databaseEntities = await DbLocatorDb
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .Include(d => d.DatabaseType)
                .ToListAsync();

            var databases = databaseEntities
                .Select(d => new Database(
                    d.DatabaseId,
                    d.DatabaseName,
                    new DatabaseType(
                        d.DatabaseType.DatabaseTypeId,
                        d.DatabaseType.DatabaseTypeName
                    ),
                    new DatabaseServer(
                        d.DatabaseServer.DatabaseServerId,
                        d.DatabaseServer.DatabaseServerName,
                        d.DatabaseServer.DatabaseServerIpaddress
                    ),
                    (Status)d.DatabaseStatusId,
                    d.UseTrustedConnection
                ))
                .ToList();

            return databases;
        }

        public async Task UpdateDatabase(
            int databaseId,
            string databaseName,
            string databaseUserName,
            int databaseServerId,
            byte databaseTypeId,
            Status databaseStatus
        )
        {
            var databaseEntity =
                await DbLocatorDb
                    .Set<DatabaseEntity>()
                    .FirstOrDefaultAsync(d => d.DatabaseId == databaseId)
                ?? throw new InvalidOperationException("Database not found.");

            var oldDatabaseName = databaseEntity.DatabaseName;
            var oldDatabaseUser = databaseEntity.DatabaseUser;

            databaseEntity.DatabaseName = databaseName;
            databaseEntity.DatabaseUser = databaseUserName;
            databaseEntity.DatabaseServerId = databaseServerId;
            databaseEntity.DatabaseTypeId = databaseTypeId;
            databaseEntity.DatabaseStatusId = (byte)databaseStatus;

            DbLocatorDb.Update(databaseEntity);
            await DbLocatorDb.SaveChangesAsync();

            var commands = new List<string>();
            if (oldDatabaseName != databaseName)
                commands.Add($"ALTER DATABASE {oldDatabaseName} MODIFY NAME = {databaseName}");

            if (oldDatabaseUser != databaseUserName)
                commands.Add(
                    $"USE {databaseName}; ALTER USER {oldDatabaseUser} WITH NAME = {databaseUserName}"
                );

            foreach (var commandText in commands)
            {
                using var command = DbLocatorDb.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await DbLocatorDb.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();
        }

        public async Task DeleteDatabase(int databaseId)
        {
            var databaseEntity =
                await DbLocatorDb.Set<DatabaseEntity>().FindAsync(databaseId)
                ?? throw new InvalidOperationException("Database not found.");

            DbLocatorDb.Set<DatabaseEntity>().Remove(databaseEntity);
            await DbLocatorDb.SaveChangesAsync();

            var commands = new[]
            {
                $"DROP DATABASE {databaseEntity.DatabaseName}",
                $"DROP LOGIN {databaseEntity.DatabaseUser}",
            };

            foreach (var commandText in commands)
            {
                using var command = DbLocatorDb.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await DbLocatorDb.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();
        }

        private async Task SwapBackToDbLocatorDatabase()
        {
            using var swapCommand = DbLocatorDb.Database.GetDbConnection().CreateCommand();
            swapCommand.CommandText = "use [DbLocator]";
            await DbLocatorDb.Database.OpenConnectionAsync();
            await swapCommand.ExecuteNonQueryAsync();
        }
    }
}
