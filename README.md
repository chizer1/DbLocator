# DbLocator

DbLocator is a library designed to simplify database interactions for multi-database tenant applications on SQL Server by managing and cataloging multiple separate database connections for each tenant.

## Documentation

📚 Full documentation is available at [https://chizer1.github.io/DbLocator](https://chizer1.github.io/DbLocator)

## Features

- Dynamically manages the retrieval and creation of database connections.
- Enables tenants to utilize multiple databases, each dedicated to distinct functional purposes.
- Implements database-level role management for SQL Server, offering fine-grained control over built-in database roles such as read/write privileges, while also supporting trusted connections that do not require a database user.
- Facilitates horizontal scaling by distributing tenant databases across multiple servers.
- Allows for automating the creation of databases, logins, users, and roles, eliminating the need for some manual scripting.

## Limitations

- Does not manage schema definitions, modifications, or data structure enforcement.
- Does not automate SQL Server instance setup.
- Does not handle DBA tasks such as backups or database migrations.

```
                         +--------------+
                         |   DbLocator  |
                         +--------------+
                                 |
      +--------------------------------------------------+
      |                     |                            |
+------------------+   +------------------+    +------------------+
| Tenant: AcmeCorp |   | Tenant: BetaCorp |    | Tenant: GammaCorp |
+------------------+   +------------------+    +------------------+
      |                     |                            |
      +---------------------+                            |
                  |                                      |
            +-------------+                        +-------------+
            | DB Server A |                        | DB Server B |
            +-------------+                        +-------------+
                  |                                      |
         +--------+--------+                             |
         |                 |                             |
  +------------+      +-----------+                +------------+
  |  Acme DB   |      |  Beta DB  |                |  Gamma DB  |
  |    (BI)    |      |  (Client) |                |    (BI)    |
  +------------+      +-----------+                +------------+
```

## Quick Start

### 1. Add package to your .NET project

Package is available on [NuGet](https://www.nuget.org/packages/DbLocator)

```bash
dotnet add package DbLocator
```

### 2. SQL Server setup

You will need an instance of SQL Server running. For local development, you can either:

- Use the SQL Server Docker image in this repository by running `docker compose up` from the DbLocatorTests folder. This requires Docker Desktop to be installed (https://docs.docker.com/get-started/get-docker/)
- Install SQL Server directly on your machine (https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Spin up a new SQL Server instance in the cloud. **Note**: This library may not play nicely with Azure SQL as this library has code that relies on traditional SQL Server logins which Azure SQL doesn't support.

### 3. Basic Usage

```csharp
// Initialize Locator with connection string
var dbLocator = new Locator("YourConnectionString");

// Add a tenant
var tenantId = await dbLocator.AddTenant("Acme Corp", "acme", Status.Active);

// Add a database type
var databaseTypeId = await dbLocator.AddDatabaseType("Client");

// Add a database server
var databaseServerId = await dbLocator.AddDatabaseServer("Local SQL Server", null, "localhost", null, false);

// Add a database
var databaseId = await dbLocator.AddDatabase("Acme_Client", databaseServerId, databaseTypeId, Status.Active, true);

// Get a connection
using var connection = await dbLocator.GetConnection(tenantId, databaseTypeId);
```

For more detailed examples and advanced usage, please visit our [documentation](https://chizer1.github.io/DbLocator).

## Examples and Implementations

- [Example Project](https://github.com/chizer1/DbLocatorExample)
- [Full Documentation](https://chizer1.github.io/DbLocator)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
