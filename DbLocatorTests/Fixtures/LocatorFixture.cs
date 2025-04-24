using Microsoft.Data.SqlClient;
using DbLocator;
using DbLocator.Db;
using DbLocator.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DbLocatorTests.Fixtures;

public class DbLocatorFixture : IDisposable, IAsyncLifetime
{
    public Locator DbLocator { get; private set; }
    internal DbLocatorCache LocatorCache { get; private set; }
    public int LocalhostServerId { get; private set; }

    private const string connString =
        "Server=localhost;Database=DbLocator;User Id=sa;Password=1StrongPwd!!;Encrypt=True;TrustServerCertificate=True;";

    public DbLocatorFixture()
    {
        var options = Options.Create<MemoryDistributedCacheOptions>(new());
        var memCache = new MemoryDistributedCache(options, NullLoggerFactory.Instance);
        LocatorCache = new DbLocatorCache(memCache);
        DbLocator = new Locator(connString, null, memCache);
    }

    public void Dispose() { }

    public async Task InitializeAsync()
    {
        // Wait for the database server to be ready
        var maxAttempts = 30; // Increased from 10 to 30
        var attempt = 0;
        var serverReady = false;

        while (!serverReady && attempt < maxAttempts)
        {
            try
            {
                // First try to connect to the database
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connString);
                await connection.OpenAsync();
                await connection.CloseAsync();

                // If connection succeeded, proceed with server setup
                var localHostServers = await DbLocator.GetDatabaseServers();
                var localHostServer = localHostServers.FirstOrDefault(server =>
                    server.HostName == "localhost"
                );

                if (localHostServer != null)
                {
                    LocalhostServerId = localHostServer.Id;
                    serverReady = true;
                }
                else
                {
                    var databaseServerName = TestHelpers.GetRandomString();
                    var databaseServerHostName = "localhost";
                    LocalhostServerId = await DbLocator.AddDatabaseServer(
                        databaseServerName,
                        databaseServerHostName,
                        null,
                        null,
                        false
                    );
                    serverReady = true;
                }
            }
            catch (Exception)
            {
                attempt++;
                if (attempt < maxAttempts)
                {
                    await Task.Delay(2000); // Increased from 1s to 2s
                }
                else
                {
                    throw new TimeoutException(
                        "Failed to initialize database server after multiple attempts"
                    );
                }
            }
        }
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}

[CollectionDefinition("DbLocator")]
public class DbLocatorCollection : ICollectionFixture<DbLocatorFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
