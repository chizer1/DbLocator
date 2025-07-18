# Getting Started with DbLocator

## Prerequisites

- .NET 9.0 (required)
- SQL Server 2016 or later

## Installation

Add the DbLocator NuGet package to your project:

```bash
dotnet add package DbLocator
```

## Basic Setup

1. First, ensure you have a SQL Server instance running. For local development, you have a couple options:

   ### Option 1: Using Docker
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest
   ```

   ### Option 2: Local Installation
   - Download SQL Server 2022 Developer Edition from [Microsoft's website](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Install SQL Server Management Studio (SSMS) for database management
   - Ensure the SQL Server service is running and accessible

2. Initialize DbLocator with your connection string:

```csharp
using DbLocator;

// Basic initialization
var dbLocator = new Locator("YourConnectionString");

// With encryption
var dbLocator = new Locator("YourConnectionString", "YourEncryptionKey");

// With caching
var dbLocator = new Locator("YourConnectionString", "YourEncryptionKey", yourCachingMechanism);
```

## Setting Up a Multi-tenant Environment

Here's a complete example of setting up a tenant with a database, user, and role-based access:

```csharp

var tenantCode = "Acme";
var tenantId = await dbLocator.CreateTenant(
    tenantName: "Acme Corp",
    tenantCode: tenantCode,
    tenantStatus: Status.Active
);

var databaseTypeName = "Client";
var databaseTypeId = await dbLocator.CreateDatabaseType(databaseTypeName: databaseTypeName);

var databaseServerId = await dbLocator.CreateDatabaseServer(
    databaseServerName: "Database Server",
    databaseServerHostName: "localhost",
    databaseServerIpAddress: null,
    databaseServerFullyQualifiedDomainName: null,
    isLinkedServer: false
);

var databaseId = await dbLocator.CreateDatabase(
    databaseName: $"{tenantCode}_{databaseTypeName}",
    databaseServerId: databaseServerId,
    databaseTypeId: databaseTypeId,
    affectDatabase: true,
    useTrustedConnection: false
);

var databaseUserId = await dbLocator.CreateDatabaseUser(
    databaseIds: new[] { databaseId },
    userName: $"{tenantCode}_{databaseTypeName}_User",
    userPassword: "YourStrongSecure@Passw0rd",
    affectDatabase: true
);

await dbLocator.CreateDatabaseUserRole(
    databaseUserId: databaseUserId,
    userRole: DatabaseRole.DataReader,
    affectDatabase: true
);

using var connection = await dbLocator.GetConnection(
    tenantId: tenantId,
    databaseTypeId: databaseTypeId
    roles: new[] { DatabaseRole.DataReader }
);

using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM Users";

using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
    Console.WriteLine($"User: {reader["Name"]}");

```

## Available Database Roles

DbLocator supports the following SQL Server database roles:

- **DataReader**: Read-only access to all user tables
- **DataWriter**: Can insert, update, and delete data in all user tables
- **DdlAdmin**: Can create, modify, and drop database objects
- **BackupOperator**: Can perform backup and restore operations
- **SecurityAdmin**: Can manage database security settings
- **DbOwner**: Full control over the database

## Connection String Management

DbLocator handles connection strings in two ways:

1. **Role-Based Connection**:
   ```csharp
   var connection = await dbLocator.GetConnection(
        tenantId: tenantId,
        databaseTypeId: databaseTypeId
        roles: new[] { DatabaseRole.DataReader, DatabaseRole.DataWriter }
   );
   ```

2. **Trusted Connection**:
   ```csharp
   var connection = await dbLocator.GetConnection(
        tenantId: tenantId,
        databaseTypeId: databaseTypeId
   );
   ```

## Security Considerations

When setting up DbLocator, consider these security best practices:

1. Use encryption for sensitive connection information
2. Follow the principle of least privilege:
   - Use `dbcreator` role if you need to create databases
   - Use `securityadmin` role if you need to create logins
   - Use no server roles if you're just mapping to existing databases
3. Use trusted connections when possible
4. Implement proper password policies for database users
5. Regularly audit database access and permissions

## Common Use Cases

1. **Multi-tenant Applications**: Manage separate databases for each tenant while maintaining centralized control
2. **Database Sharding**: Distribute databases across multiple servers for better scalability
3. **Role-Based Access**: Implement different access levels for different user types

## Next Steps

- Learn about [Advanced Configuration](advanced-configuration.md) for more complex scenarios
- Check out the [Examples](examples.md) for common usage patterns
- Review the [API Reference](../api/) for detailed method documentation