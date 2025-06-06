# Examples

This page contains practical examples of DbLocator usage in common scenarios.

## Basic Tenant Setup

```csharp
// Initialize DbLocator
var dbLocator = new Locator("YourConnectionString");

// Create a new tenant
var tenantId = await dbLocator.AddTenant("Acme Corp", "acme", Status.Active);

// Set up their client database
var clientDbTypeId = await dbLocator.AddDatabaseType("Client");
var serverId = await dbLocator.AddDatabaseServer("Local SQL", null, "localhost", null, false);
var dbId = await dbLocator.AddDatabase("Acme_Client", serverId, clientDbTypeId, Status.Active, true);

// Get a connection to their client database
using var connection = await dbLocator.GetConnection(tenantId, clientDbTypeId);
```

## Multi-Database Tenant

```csharp
// Set up multiple database types for a tenant
var clientDbTypeId = await dbLocator.AddDatabaseType("Client");
var biDbTypeId = await dbLocator.AddDatabaseType("BI");
var reportingDbTypeId = await dbLocator.AddDatabaseType("Reporting");

// Add databases for each type
var clientDbId = await dbLocator.AddDatabase("Acme_Client", serverId, clientDbTypeId, Status.Active, true);
var biDbId = await dbLocator.AddDatabase("Acme_BI", serverId, biDbTypeId, Status.Active, true);
var reportingDbId = await dbLocator.AddDatabase("Acme_Reporting", serverId, reportingDbTypeId, Status.Active, true);

// Get connections to different databases
using var clientConnection = await dbLocator.GetConnection(tenantId, clientDbTypeId);
using var biConnection = await dbLocator.GetConnection(tenantId, biDbTypeId);
using var reportingConnection = await dbLocator.GetConnection(tenantId, reportingDbTypeId);
```

## Using with Entity Framework Core

```csharp
// In your Startup.cs or Program.cs
services.AddDbContext<MyDbContext>(options =>
{
    var dbLocator = new Locator(Configuration.GetConnectionString("DbLocator"));
    var connection = dbLocator.GetConnection(tenantId, clientDbTypeId).GetAwaiter().GetResult();
    options.UseSqlServer(connection);
});
```

## Role-Based Access

```csharp
// Get a connection with specific database roles
using var readerConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId, 
    new[] { DatabaseRole.DataReader }
);

using var writerConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId, 
    new[] { DatabaseRole.DataReader, DatabaseRole.DataWriter }
);
```

## Error Handling

```csharp
try
{
    var connection = await dbLocator.GetConnection(tenantId, databaseTypeId);
    // Use the connection
}
catch (DatabaseNotFoundException ex)
{
    // Handle case where database doesn't exist
    Console.WriteLine($"Database not found: {ex.Message}");
}
catch (ConnectionFailedException ex)
{
    // Handle connection failures
    Console.WriteLine($"Connection failed: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other exceptions
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Using with Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddSingleton<Locator>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new Locator(
        configuration.GetConnectionString("DbLocator"),
        configuration["DbLocator:EncryptionKey"]
    );
});

// In your service
public class MyService
{
    private readonly Locator _dbLocator;

    public MyService(Locator dbLocator)
    {
        _dbLocator = dbLocator;
    }

    public async Task DoSomethingAsync(string tenantCode)
    {
        var connection = await _dbLocator.GetConnection(tenantCode, "Client");
        // Use the connection
    }
}
``` 