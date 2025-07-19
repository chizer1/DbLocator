# DbLocator Examples

This guide covers possibe real-world examples for using **DbLocator**.

---

## Custom Connection Provider

You can implement a custom provider if you need additional control:

```csharp
public class CustomConnectionProvider : IConnectionProvider
{
    private readonly ILogger<CustomConnectionProvider> _logger;

    public CustomConnectionProvider(ILogger<CustomConnectionProvider> logger)
        => _logger = logger;

    public async Task<SqlConnection> GetConnectionAsync(
        string connectionString,
        bool useTrustedConnection = false)
    {
        try
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection failed");
            throw;
        }
    }
}
```

## Integrating with Entity Framework Core

```csharp
public class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly Locator _dbLocator;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantContext tenantContext,
        Locator dbLocator)
        : base(options)
    {
        _tenantContext = tenantContext;
        _dbLocator = dbLocator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connection = _dbLocator.GetConnection(
                _tenantContext.TenantId,
                "Client",
                new[] { DatabaseRole.DataReader }
            ).GetAwaiter().GetResult();

            optionsBuilder.UseSqlServer(connection);
        }
    }
}
```

## Using in Background Services

```csharp
public class DatabaseMaintenanceService : BackgroundService
{
    private readonly Locator _dbLocator;
    private readonly ILogger<DatabaseMaintenanceService> _logger;

    public DatabaseMaintenanceService(Locator dbLocator, ILogger<DatabaseMaintenanceService> logger)
    {
        _dbLocator = dbLocator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var databases = await _dbLocator.GetDatabases(Status.Active);
                foreach (var db in databases)
                    await PerformMaintenanceAsync(db);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maintenance error");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task PerformMaintenanceAsync(Database db)
    {
        // Rebuild indexes, update stats, check integrity, etc.
    }
}
```

## Example: Multi-Database Tenant Setup
```csharp
var clientDbTypeId = await dbLocator.AddDatabaseType("Client");
var biDbTypeId = await dbLocator.AddDatabaseType("BI");
var reportingDbTypeId = await dbLocator.AddDatabaseType("Reporting");

var clientDbId = await dbLocator.CreateDatabase("Acme_Client", serverId, clientDbTypeId, Status.Active, true, false);
var biDbId = await dbLocator.CreateDatabase("Acme_BI", serverId, biDbTypeId, Status.Active, true, false);
var reportingDbId = await dbLocator.CreateDatabase("Acme_Reporting", serverId, reportingDbTypeId, Status.Active, true, false);

var userId = await dbLocator.CreateDatabaseUser(
    new[] { clientDbId, biDbId, reportingDbId },
    "acme_user", "Strong@Passw0rd", true);

await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DataReader, true);   // Client DB
await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DataWriter, true);   // BI DB
await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DbOwner, true);      // Reporting DB

using var clientConnection = await dbLocator.GetConnection(tenantId, clientDbTypeId, new[] { DatabaseRole.DataReader });
using var biConnection = await dbLocator.GetConnection(tenantId, biDbTypeId, new[] { DatabaseRole.DataWriter });
using var reportingConnection = await dbLocator.GetConnection(tenantId, reportingDbTypeId, new[] { DatabaseRole.DbOwner });
```

## Next Steps

- Review the [API Reference](../api/) for detailed method documentation
