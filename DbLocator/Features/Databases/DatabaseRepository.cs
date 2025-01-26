using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases
{
    internal class DatabaseRepository(IDbContextFactory<DbLocatorContext> dbContextFactory)
        : IDatabaseRepository
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
            await using var dbContext = dbContextFactory.CreateDbContext();

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

            await dbContext.Set<DatabaseEntity>().AddAsync(database);
            await dbContext.SaveChangesAsync();

            foreach (var commandText in commands)
            {
                using var command = dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();

            return database.DatabaseId;
        }

        private async Task EnsureDatabaseExists(string databaseName, bool createDatabase)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText =
                $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
            await dbContext.Database.OpenConnectionAsync();
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
            await using var dbContext = dbContextFactory.CreateDbContext();

            using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText =
                $"SELECT COUNT(*) FROM sys.server_principals WHERE name = '{databaseUser}'";
            await dbContext.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            if (result is int count && count > 0)
                throw new InvalidOperationException($"User {databaseUser} already exists.");
        }

        public async Task<Database> GetDatabase(int databaseId)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var database =
                await dbContext
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
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseEntities = await dbContext
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
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseEntity =
                await dbContext
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

            dbContext.Update(databaseEntity);
            await dbContext.SaveChangesAsync();

            var commands = new List<string>();
            if (oldDatabaseName != databaseName)
                commands.Add($"ALTER DATABASE {oldDatabaseName} MODIFY NAME = {databaseName}");

            if (oldDatabaseUser != databaseUserName)
                commands.Add(
                    $"USE {databaseName}; ALTER USER {oldDatabaseUser} WITH NAME = {databaseUserName}"
                );

            foreach (var commandText in commands)
            {
                using var command = dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();
        }

        public async Task DeleteDatabase(int databaseId)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseEntity =
                await dbContext.Set<DatabaseEntity>().FindAsync(databaseId)
                ?? throw new InvalidOperationException("Database not found.");

            dbContext.Set<DatabaseEntity>().Remove(databaseEntity);
            await dbContext.SaveChangesAsync();

            var commands = new[]
            {
                $"DROP DATABASE {databaseEntity.DatabaseName}",
                $"DROP LOGIN {databaseEntity.DatabaseUser}",
            };

            foreach (var commandText in commands)
            {
                using var command = dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            await SwapBackToDbLocatorDatabase();
        }

        private async Task SwapBackToDbLocatorDatabase()
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            using var swapCommand = dbContext.Database.GetDbConnection().CreateCommand();
            swapCommand.CommandText = "use [DbLocator]";
            await dbContext.Database.OpenConnectionAsync();
            await swapCommand.ExecuteNonQueryAsync();
        }
    }
}
