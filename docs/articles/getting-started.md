# Getting Started with DbLocator

This guide will help you get started with DbLocator in your .NET application.

## Installation

Add the DbLocator NuGet package to your project:

```bash
dotnet add package DbLocator
```

## Basic Usage

Here's a simple example of how to use DbLocator:

```csharp
using DbLocator;

// Initialize the DbLocator
var dbLocator = new DbLocator();

// Get a database connection
var connection = await dbLocator.GetConnectionAsync("YourDatabaseName");
```

## Configuration

DbLocator can be configured through your application's configuration system. Add the following to your `appsettings.json`:

```json
{
  "DbLocator": {
    "DefaultConnection": "YourConnectionString",
    "Databases": {
      "Database1": "ConnectionString1",
      "Database2": "ConnectionString2"
    }
  }
}
```

## Next Steps

- Check out the [API Reference](../api/) for detailed information about available methods and properties
- Learn about [Advanced Configuration](advanced-configuration.md)
- See [Examples](examples.md) for more usage scenarios 