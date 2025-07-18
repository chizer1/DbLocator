# Advanced Configuration

This guide covers advanced configuration options and patterns for DbLocator.

## Custom Connection Provider

DbLocator allows you to implement custom connection providers by implementing the `IConnectionProvider` interface:

```csharp
public interface IConnectionProvider
{
    Task<SqlConnection> GetConnectionAsync(
        string connectionString,
        bool useTrustedConnection = false
    );
}
```

Example implementation:

```csharp
public class CustomConnectionProvider(ILogger<CustomConnectionProvider> logger) : IConnectionProvider
{
    private readonly ILogger<CustomConnectionProvider> _logger = logger;

    public async Task<SqlConnection> GetConnectionAsync(
        string connectionString,
        bool useTrustedConnection = false
    )
    {
        try
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection");
            throw;
        }
    }
}
```

## Using with Entity Framework Core

DbLocator can be integrated with Entity Framework Core for multi-tenant applications:

```csharp
public class TenantDbContext(
    DbContextOptions<TenantDbContext> options,
    ITenantContext tenantContext,
    Locator dbLocator
) : DbContext(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly Locator _dbLocator = dbLocator;

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

DbLocator can be used in background services for maintenance tasks:

```csharp
public class DatabaseMaintenanceService(
    Locator dbLocator,
    ILogger<DatabaseMaintenanceService> logger
) : BackgroundService
{
    private readonly Locator _dbLocator = dbLocator;
    private readonly ILogger<DatabaseMaintenanceService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var databases = await _dbLocator.GetDatabases(Status.Active);

                foreach (var database in databases)
                    await PerformMaintenanceAsync(database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database maintenance");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task PerformMaintenanceAsync(Database database)
    {
        // Example maintenance tasks:
        // - Update statistics
        // - Rebuild indexes
        // - Check database integrity
        // - Backup verification
    }
}
```

## Next Steps

- Review the [API Reference](../api/) for detailed method documentation
- Check out the [Examples](examples.md) for common usage patterns
- Learn about [Getting Started](getting-started.md) for basic setup