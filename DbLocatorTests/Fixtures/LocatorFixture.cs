using System.Diagnostics;
using DbLocator;
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

    private readonly string DockerFile;

    public string ConnectionString =
        "Server=localhost;Database=DbLocator;User Id=sa;Password=1StrongPwd!!;Encrypt=True;TrustServerCertificate=True;";

    public DbLocatorFixture()
    {
        // Stop the docker container
        DockerFile = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..",
            "..",
            "..",
            "docker-compose.yml"
        );
        DockerFile = Path.GetFullPath(DockerFile);

        var command = $"docker compose -f \"{DockerFile}\" up --detach";
        WaitForCommand(command);

        var options = Options.Create<MemoryDistributedCacheOptions>(new());
        var memCache = new MemoryDistributedCache(options, NullLoggerFactory.Instance);
        LocatorCache = new DbLocatorCache(memCache);
        DbLocator = new Locator(ConnectionString, null, memCache);
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
        var command = $"docker compose -f \"{DockerFile}\" down -v --remove-orphans";
        WaitForCommand(command);

        command = "docker volume prune --force";
        WaitForCommand(command);

        await Task.CompletedTask;
    }

    private void WaitForCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sh",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            },
        };
        process.Start();
        process.WaitForExit();
        var exitCode = process.ExitCode;
        if (exitCode != 0)
        {
            var err = process.StandardOutput.ReadToEnd();

            if (string.IsNullOrEmpty(err))
            {
                err = process.StandardError.ReadToEnd();
            }

            throw new Exception(
                $"Command '{command}' failed with exit code {exitCode}. Error: {err}"
            );
        }
    }
}

[CollectionDefinition("DbLocator")]
public class DbLocatorCollection : ICollectionFixture<DbLocatorFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
