# Getting Started with DbLocator

DbLocator is a library designed to simplify database interactions for multi-database tenant applications on SQL Server. This guide will help you get started with DbLocator in your .NET application.

## Prerequisites

- .NET 9.0 (required)
- SQL Server 2016 or later

## Installation

Add the DbLocator NuGet package to your project:

```bash
dotnet add package DbLocator
```

## Key Features

- **Multi-tenant Database Management**: Manage multiple databases for different tenants with ease
- **Role-Based Access Control**: Implement fine-grained access control using SQL Server database roles
- **Database Server Management**: Support for multiple server identification methods (hostname, FQDN, IP)
- **Connection Management**: Secure connection handling with SQL Server authentication
- **Distributed Caching**: Optional caching support for improved performance
- **Data Encryption**: Built-in encryption for sensitive connection information

## Basic Setup

1. First, ensure you have a SQL Server instance running. For local development, you have several options:

   ### Option 1: Using Docker (Recommended for Development)
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

// With encryption (recommended for production)
var dbLocator = new Locator("YourConnectionString", "YourEncryptionKey");

// With caching (for better performance)
var dbLocator = new Locator("YourConnectionString", "YourEncryptionKey", cache);
```

## Setting Up a Multi-tenant Environment

Here's a complete example of setting up a tenant with a database, user, and role-based access:

```csharp
// Add a tenant
var tenantId = await dbLocator.AddTenant("Acme Corp", "acme", Status.Active);

// Add a database type
var databaseTypeId = await dbLocator.AddDatabaseType("Client");

// Add a database server
var databaseServerId = await dbLocator.CreateDatabaseServer(
    "Local SQL Server",    // Name
    "localhost",          // HostName
    "127.0.0.1",         // IP Address
    "localhost.local",    // Fully Qualified Domain Name
    false                 // Is Linked Server
);

// Add a database
var databaseId = await dbLocator.CreateDatabase(
    "Acme_Client",        // Database name
    databaseServerId,     // Server ID
    databaseTypeId,       // Database type ID
    Status.Active,        // Status
    true                  // Auto-create database
);

// Create a database user
var userId = await dbLocator.CreateDatabaseUser(
    new[] { databaseId },  // Database IDs
    "acme_user",          // Username
    "Strong@Passw0rd",    // Password
    true                  // Create user on database server
);

// Assign roles to the user
await dbLocator.CreateDatabaseUserRole(
    userId,               // User ID
    DatabaseRole.DataReader,  // Role
    true                  // Update user on database server
);

// Get a SqlConnection with specific role
using var connection = await dbLocator.GetConnection(
    tenantId, 
    databaseTypeId,
    new[] { DatabaseRole.DataReader }  // Required roles
);
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

DbLocator handles connection strings in several ways:

1. **Basic Connection**: Uses SQL Server authentication
   ```csharp
   var connection = await dbLocator.GetConnection(tenantId, databaseTypeId);
   ```

2. **Role-Based Connection**: Ensures user has specific roles
   ```csharp
   var connection = await dbLocator.GetConnection(
       tenantId, 
       databaseTypeId,
       new[] { DatabaseRole.DataReader, DatabaseRole.DataWriter }
   );
   ```

3. **Trusted Connection**: Uses Windows authentication
   ```csharp
   var databaseId = await dbLocator.CreateDatabase(
       "MyDatabase",
       serverId,
       typeId,
       useTrustedConnection: true
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