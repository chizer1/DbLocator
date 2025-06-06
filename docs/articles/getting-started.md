# Getting Started with DbLocator

DbLocator is a library designed to simplify database interactions for multi-database tenant applications on SQL Server. This guide will help you get started with DbLocator in your .NET application.

## Prerequisites

- .NET 6.0 or later
- SQL Server instance (local or remote)
- Basic understanding of SQL Server and multi-tenant applications

## Installation

Add the DbLocator NuGet package to your project:

```bash
dotnet add package DbLocator
```

## Basic Setup

1. First, ensure you have a SQL Server instance running. For local development, you can:
   - Use the SQL Server Docker image from the DbLocatorTests folder
   - Install SQL Server locally
   - Use a cloud-based SQL Server instance

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

## Creating Your First Tenant

Here's a basic example of setting up a tenant with a database:

```csharp
// Add a tenant
var tenantId = await dbLocator.AddTenant("Acme Corp", "acme", Status.Active);

// Add a database type
var databaseTypeId = await dbLocator.AddDatabaseType("Client");

// Add a database server
var databaseServerId = await dbLocator.AddDatabaseServer(
    "Local SQL Server",    // Name
    null,                  // Instance name (null for default)
    "localhost",          // Hostname
    null,                 // Port (null for default)
    false                 // Is trusted connection
);

// Add a database
var databaseId = await dbLocator.AddDatabase(
    "Acme_Client",        // Database name
    databaseServerId,     // Server ID
    databaseTypeId,       // Database type ID
    Status.Active,        // Status
    true                  // Auto-create database
);

// Get a connection
using var connection = await dbLocator.GetConnection(tenantId, databaseTypeId);
```

## Security Considerations

When setting up DbLocator, consider these security best practices:

1. Use encryption for sensitive connection information
2. Follow the principle of least privilege:
   - Use `dbcreator` role if you need to create databases
   - Use `securityadmin` role if you need to create logins
   - Use no server roles if you're just mapping to existing databases

## Next Steps

- Learn about [Advanced Configuration](advanced-configuration.md) for more complex scenarios
- Check out the [Examples](examples.md) for common usage patterns
- Review the [API Reference](../api/) for detailed method documentation 