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
        var localHostServers = await DbLocator.GetDatabaseServers();
        var localHostServer = localHostServers.FirstOrDefault(server =>
            server.HostName == "localhost"
        );

        if (localHostServer != null)
        {
            LocalhostServerId = localHostServer.Id;
            return;
        }

        var databaseServerName = TestHelpers.GetRandomString();
        var databaseServerHostName = "localhost";
        LocalhostServerId = await DbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerHostName,
            null,
            null,
            false
        );
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
