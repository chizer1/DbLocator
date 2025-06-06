# Advanced Configuration

This guide covers advanced configuration options for DbLocator.

## Custom Connection Providers

DbLocator supports custom connection providers. Here's how to implement one:

```csharp
public class CustomConnectionProvider : IConnectionProvider
{
    public async Task<IDbConnection> GetConnectionAsync(string databaseName)
    {
        // Your custom connection logic here
    }
}
```

## Connection Pooling

Configure connection pooling in your `appsettings.json`:

```json
{
  "DbLocator": {
    "ConnectionPooling": {
      "MinSize": 5,
      "MaxSize": 20,
      "Timeout": 30
    }
  }
}
```

## Security

Learn about securing your database connections and managing credentials. 