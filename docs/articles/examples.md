# Examples

This page contains practical examples of DbLocator usage in common scenarios.

## Basic Tenant Setup

```csharp
// Initialize DbLocator with encryption
var dbLocator = new Locator(
    "YourConnectionString",
    "YourEncryptionKey"
);

// Create a new tenant
var tenantId = await dbLocator.AddTenant(
    "Acme Corp",     // Name
    "acme",          // Code
    Status.Active    // Status
);

// Set up their client database
var clientDbTypeId = await dbLocator.AddDatabaseType("Client");

// Add a database server with all identification methods
var serverId = await dbLocator.CreateDatabaseServer(
    "Local SQL",     // Name
    "localhost",     // HostName
    "127.0.0.1",    // IP Address
    "localhost.local", // FQDN
    false           // Is Linked Server
);

// Create the database
var dbId = await dbLocator.CreateDatabase(
    "Acme_Client",   // Database name
    serverId,        // Server ID
    clientDbTypeId,  // Database type ID
    Status.Active,   // Status
    autoCreateDatabase: true,           // Auto-create database (creates the database if it doesn't exist)
    useTrustedConnection: false           // Use Windows authentication (false = SQL Server authentication)
);

// Create a database user
var userId = await dbLocator.CreateDatabaseUser(
    new[] { dbId },  // Database IDs
    "acme_user",     // Username
    "Strong@Passw0rd", // Password
    true            // Create user on database server (creates the login and user)
);

// Assign roles to the user
await dbLocator.CreateDatabaseUserRole(
    userId,          // User ID
    DatabaseRole.DataReader, // Role
    true            // Update user on database server (grants the role)
);

// Get a SqlConnection with specific role
using var connection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId,
    new[] { DatabaseRole.DataReader }
);
```

## Multi-Database Tenant

```csharp
// Set up multiple database types for a tenant
var clientDbTypeId = await dbLocator.AddDatabaseType("Client");
var biDbTypeId = await dbLocator.AddDatabaseType("BI");
var reportingDbTypeId = await dbLocator.AddDatabaseType("Reporting");

// Add databases for each type
var clientDbId = await dbLocator.CreateDatabase(
    "Acme_Client", 
    serverId, 
    clientDbTypeId, 
    Status.Active, 
    autoCreateDatabase: true,           // Auto-create database
    useTrustedConnection: false           // Use SQL Server authentication
);

var biDbId = await dbLocator.CreateDatabase(
    "Acme_BI", 
    serverId, 
    biDbTypeId, 
    Status.Active, 
    autoCreateDatabase: true,           // Auto-create database
    useTrustedConnection: false           // Use SQL Server authentication
);

var reportingDbId = await dbLocator.CreateDatabase(
    "Acme_Reporting", 
    serverId, 
    reportingDbTypeId, 
    Status.Active, 
    autoCreateDatabase: true,           // Auto-create database
    useTrustedConnection: false           // Use SQL Server authentication
);

// Create a user with access to all databases
var userId = await dbLocator.CreateDatabaseUser(
    new[] { clientDbId, biDbId, reportingDbId },
    "acme_user",
    "Strong@Passw0rd",
    true            // Create user on database server
);

// Assign different roles for different databases
await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DataReader, true);  // Client DB
await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DataWriter, true);  // BI DB
await dbLocator.CreateDatabaseUserRole(userId, DatabaseRole.DbOwner, true);     // Reporting DB

// Get connections to different databases with appropriate roles
using var clientConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId,
    new[] { DatabaseRole.DataReader }
);

using var biConnection = await dbLocator.GetConnection(
    tenantId, 
    biDbTypeId,
    new[] { DatabaseRole.DataWriter }
);

using var reportingConnection = await dbLocator.GetConnection(
    tenantId, 
    reportingDbTypeId,
    new[] { DatabaseRole.DbOwner }
);
```

## Role-Based Access

```csharp
// Get a connection with specific database roles
using var readerConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId, 
    new[] { DatabaseRole.DataReader }
);

// Get a connection with multiple roles
using var writerConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId, 
    new[] { DatabaseRole.DataReader, DatabaseRole.DataWriter }
);

// Get a connection with administrative access
using var adminConnection = await dbLocator.GetConnection(
    tenantId, 
    clientDbTypeId, 
    new[] { DatabaseRole.DbOwner }
);
```