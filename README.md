# About

The purpose of this library is to simplify database interactions for applications managing multi-tenant environments with multiple database connections.

What it can do:
1. Supports horizontal scaling by distributing tenant databases across multiple servers.
2. Instead of manually managing SQL connections, the class provides a structured way to retrieve and create database connections dynamically based on tenant and database type.
3. Each tenant might require multiple databases, each serving a distinct functional purpose. Instead of a single database handling all tenant data, the system can provision separate logical databases tailored to different workloads.

What it can't do:
1. It does not handle user roles, permissions, or access control at the SQL Server level. 
2. Since the library focuses on managing database connections rather than the content of tenant databases, it inherently does not handle schema definitions, modifications, or data structure enforcement.

Below are some graphs to help visualize what it does.
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

![Screenshot from 2025-01-25 15-28-20](https://github.com/user-attachments/assets/6fbd631d-e637-4db4-b057-c02c1ba38edb)

## How to run

### 1. Add GitHub package source 
`dotnet nuget add source https://nuget.pkg.github.com/chizer1/index.json --name DbLocator --username <YourGitHubUsername> --password <YourGitHubPAT>`

### 2. Add package to your .Net project
`dotnet add package DbLocator`

### 3. SQL Server setup
You will need an instance of SQL Server running. For local development, you can either:
  - Use the SQL Server image in this repository by running `docker compose up` from the root. This requires Docker Desktop to be installed (https://docs.docker.com/get-started/get-docker/)
  - Install SQL Server directly on your machine (https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### 4. Initialization 

After installing the DbLocator package and setting up SQL Server, you can start using the library. The main class of the library is `Locator`, which can be initialized like this:

```csharp
Locator dbLocator = new("{YourConnectionString}");
// Example ConnectionString: "Server=localhost;Database=DbLocator;User Id=sa;Password=1StrongPwd!!;Encrypt=True;TrustServerCertificate=True;"
```

After initializing the Locator object and running your application, it will automatically create the DbLocator database and you can start using its methods.

### 5. Code example

```csharp

var tenantCode = "acme";
var tenantId = await dbLocator.AddTenant("Acme Corp", tenantCode, Status.Active);

var databaseTypeId = await dbLocator.AddDatabaseType("Client");

var databaseServerId = await dbLocator.AddDatabaseServer("localhost", "127.0.0.1");

var databaseId = await dbLocator.AddDatabase("Acme_Client", "acme_client_user", databaseServerId, databaseTypeId, Status.Active);

var connectionId = await dbLocator.AddConnection(tenantId, databaseId);

// several ways to start a SQL Connection
SqlConnection connection1 = await dbLocator.GetConnection(connectionId);
SqlConnection connection2 = await dbLocator.GetConnection(tenantId, databaseTypeId);
SqlConnection connection3 = await dbLocator.GetConnection(tenantCode, databaseTypeId);

```

## Implementations

- [https://github.com/chizer1/DbLocatorExample](https://github.com/chizer1/DbLocatorExample)
