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

2. Initialize DbLocator:

```csharp
using DbLocator;

// Basic initialization
var dbLocator = new Locator("{YourConnectionString}");

// With encryption
var dbLocator = new Locator("{YourConnectionString}", "{YourEncryptionKey}");

// With caching
var dbLocator = new Locator("{YourConnectionString}",  yourCachingMechanism);

// With encryption and caching
var dbLocator = new Locator("{YourConnectionString}", "{YourEncryptionKey}", yourCachingMechanism);
```

## Setting Up a Multi-tenant Environment

1. Set up a tenant with a database, user, and role-based access:

Setup
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

var databaseName = $"{tenantCode}_{databaseTypeName}";
var databaseId = await dbLocator.CreateDatabase(
    databaseName: databaseName,
    databaseServerId: databaseServerId,
    databaseTypeId: databaseTypeId,
    affectDatabase: true,
    useTrustedConnection: false
);

var databaseUserId = await dbLocator.CreateDatabaseUser(
    databaseIds: new[] { databaseId },
    userName: $"{databaseName}_User",
    userPassword: "SecurePassword",
    affectDatabase: true
);

await dbLocator.CreateDatabaseUserRole(
    databaseUserId: databaseUserId,
    userRole: DatabaseRole.DataReader,
    affectDatabase: true
);
```

Run script in database
```sql
CREATE TABLE User (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

INSERT INTO User (Name) VALUES ('Alice'), ('Bob'), ('Charlie');
```

Connect and query
```csharp
using var connection = await dbLocator.GetConnection(
    tenantId: tenantId,
    databaseTypeId: databaseTypeId
    roles: new[] { DatabaseRole.DataReader }
);

using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM User";

using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
    Console.WriteLine($"User: {reader["Name"]}");
```

2. Set up a tenant with a database that has a trusted connection.

Setup
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
    useTrustedConnection: true
);

await dbLocator.CreateConnection(
    tenantId: tenantId,
    databaseTypeId: databaseTypeId
);
```

Run script in database
```sql
CREATE TABLE User (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

INSERT INTO User (Name) VALUES ('Alice'), ('Bob'), ('Charlie');
```

Connect and query
```csharp
using var connection = await dbLocator.GetConnection(
    tenantId: tenantId,
    databaseTypeId: databaseTypeId
);

using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM Users";

using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
    Console.WriteLine($"User: {reader["Name"]}");
```

## Next Steps

- Check out the [Examples](examples.md) for common usage patterns
- Review the [API Reference](../api/) for detailed method documentation