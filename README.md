# DbLocator

DbLocator is a .NET library that simplifies database interactions for multi-database tenant applications on SQL Server.

## 📊 Architecture

```mermaid
graph TD
    %% Groups
    subgraph DbLocator

        subgraph Tenants
            T1[Acme Corp]
            T2[Beta Corp]
            T3[Gamma Corp]
        end

        subgraph Database Types
            DT1[Client]
            DT2[Reporting]
        end

        subgraph Database Servers
            S1[Database Server 1]
            S2[Database Server 2]
        end

        subgraph Databases
            D1[Acme_Client]
            D2[Acme_Reporting]
            D3[Beta_Client]
            D4[Beta_Reporting]
            D5[Gamma_Client]
            D6[Gamma_Reporting]
        end

    end

    T1 --> DT1
    T1 --> DT2
    T2 --> DT1
    T2 --> DT2
    T3 --> DT1
    T3 --> DT2

    DT1 --> D1
    DT2 --> D2
    DT1 --> D3
    DT2 --> D4
    DT1 --> D5
    DT2 --> D6

    %% Database to Server mapping
    D1 --> S1
    D2 --> S1
    D3 --> S1
    D4 --> S1
    D5 --> S2
    D6 --> S2
```

## 📚 Documentation

Full documentation is available at [https://chizer1.github.io/DbLocator](https://chizer1.github.io/DbLocator)

## 🚀 Quick Start

### Installation

The package is available on [NuGet](https://www.nuget.org/packages/DbLocator):

```bash
dotnet add package DbLocator
```

### Basic usage

```csharp
var connectionString = "{yourConnectionString}";
var dbLocator = new Locator(connectionString);

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

## 📖 Examples

- [DbLocatorExample](https://github.com/chizer1/DbLocatorExample)
- [aspire-multi-tenant-starter](https://github.com/chizer1/aspire-multi-tenant-starter)

## 🤝 Contributing

I welcome contributions! Here's how you can help:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please read [Contributing Guidelines](CONTRIBUTING.md) for more details.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
