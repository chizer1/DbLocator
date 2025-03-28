# DbLocator

DbLocator is a library designed to simplify database interactions for multi-tenant applications by managing and cataloging multiple database connections.  

## Features  
- Dynamically retrieves and creates database connections.
- Allows tenants to have multiple databases, each serving distinct functional purposes.  
- Implements database-level role management for SQL Server, enabling fine-grained control over built-in database roles such as read/write privileges.
- Supports horizontal scaling by distributing tenant databases across multiple servers.  

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

## How to run

### 1. Add package to your .Net project
Package is available on nuget.org (https://www.nuget.org/packages/DbLocator)
```csharp
dotnet add package DbLocator
```

### 2. SQL Server setup
You will need an instance of SQL Server running. For local development, you can either:
  - Use the SQL Server Docker image in this repository by running `docker compose up` from the root. This requires Docker Desktop to be installed (https://docs.docker.com/get-started/get-docker/)
  - Install SQL Server directly on your machine (https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
  - Spin up a new SQL Server instance in the cloud. **Note**: This library may not play nicely with Azure SQL as this library has code that relies on traditional SQL Server logins which Azure SQL doesn't support.

### 3. Initialization 

After installing the DbLocator package and setting up SQL Server, you can start using the library. The main class of the library is `Locator`, which can be initialized in several ways:

```csharp
Locator dbLocator = new("YourConnectionString");

Locator dbLocator = new("YourConnectionString", "EncryptionKey");

// full example for caching omitted for brevity
IDistributedCache cache = builder
    .Services.BuildServiceProvider()
    .GetRequiredService<IDistributedCache>();

Locator dbLocator = new("YourConnectionString", "EncryptionKey", cache);
```
In a real world scenario, you probably wouldn't want to connect an sysadmin login to this library for security purposes (Principle of Least Privilege).
You would want to create a login with these server level roles:
1. **dbcreator**: If you want to create databases from this library
2. **securityadmin**: If you want to create logins from this library.
3. No server level roles, if you don't want to autocreate databases or logins and just map to existing ones. 

After initializing the Locator object and running your application, it will automatically create the DbLocator database and you can start using its methods.

### 4. Code example

```csharp

var tenantCode = "acme";
var tenantId = await dbLocator.AddTenant("Acme Corp", tenantCode, Status.Active);

var databaseTypeId = await dbLocator.AddDatabaseType("Client");

// using hostname to connect to server (localhost if using docker image from repo)
var databaseServerId = await dbLocator.AddDatabaseServer("Docker SQL Server", null, "localhost", null, false); 

var databaseId = await dbLocator.AddDatabase("Acme_Client", databaseServerId, databaseTypeId, Status.Active, true);

// if using a trusted connection, specifying a user is not required
var databaseUserId = await dbLocator.AddDatabaseUser(databaseId, "acme_client_user", "acme_client_user_password", true);

var connectionId = await dbLocator.AddConnection(tenantId, databaseId);

// several ways to start a SQL Connection
SqlConnection connection1 = await dbLocator.GetConnection(connectionId);
SqlConnection connection2 = await dbLocator.GetConnection(tenantId, databaseTypeId);
SqlConnection connection3 = await dbLocator.GetConnection(tenantCode, databaseTypeId);
SqlConnection connection4 = await dbLocator.GetConnection(tenantCode, databaseTypeId, new[] { DatabaseRole.DataReader });
```

### 5. Linked Servers

If you plan to use multiple database servers with the DbLocator library, you may want to connect them to the server that hosts the DbLocator database. While the library does not automatically create linked servers for you, once set up, you can leverage them for seamless cross-server connections.

More about Linked Servers (https://learn.microsoft.com/en-us/sql/relational-databases/linked-servers/linked-servers-database-engine?view=sql-server-ver16)

Here is an example if you want set it up:

```sql

-- Run the following stored procedures from the server that hosts the DbLocator database
exec sp_addlinkedserver 
    @server = 'RemoteServerName',  -- Name of the linked server (hostname)
    @srvproduct = '',
    @provider = 'SQLNCLI',
    @datasrc = 'RemoteServerInstance';  -- Remote SQL Server instance (ip address or fully qualified domain name)

exec sp_addlinkedsrvlogin 
    @rmtsrvname = 'RemoteServerName',  -- Name of the linked server (hostname)
    @locallogin = NULL,  
    @rmtuser = 'sa', -- which ever user you want to connect for linked server access
    @rmtpassword = 'YourRemotePassword'; -- that user's password

```

## Implementations

- [https://github.com/chizer1/DbLocatorExample](https://github.com/chizer1/DbLocatorExample)
