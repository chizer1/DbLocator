using DbLocator;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
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
                using var connection = new SqlConnection(connString);
                await connection.OpenAsync();
                await connection.CloseAsync();

                var localHostServer = await DbLocator.AddDatabaseServer(
                    "localhost",
                    null,
                    "localhost",
                    null,
                    false
                );

                serverReady = true;
            }
            catch (Exception)
            {
                attempt++;
                if (attempt < maxAttempts)
                {
                    await Task.Delay(2000);
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
