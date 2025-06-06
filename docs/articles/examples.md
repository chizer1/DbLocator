# Examples

This page contains examples of common DbLocator usage scenarios.

## Basic Usage

```csharp
using DbLocator;

var dbLocator = new DbLocator();
var connection = await dbLocator.GetConnectionAsync("MyDatabase");
```

## Multiple Databases

```csharp
// Get connections to multiple databases
var db1 = await dbLocator.GetConnectionAsync("Database1");
var db2 = await dbLocator.GetConnectionAsync("Database2");
```

## Using with Entity Framework

```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(dbLocator.GetConnectionString("MyDatabase")));
```

## Error Handling

```csharp
try
{
    var connection = await dbLocator.GetConnectionAsync("MyDatabase");
}
catch (DatabaseNotFoundException ex)
{
    // Handle database not found
}
catch (ConnectionFailedException ex)
{
    // Handle connection failure
}
``` 