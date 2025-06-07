# API Documentation

Welcome to the DbLocator API documentation. This section contains detailed information about all public classes, methods, and properties in the DbLocator library.

## Namespaces

- [DbLocator](xref:DbLocator)
  - Main namespace containing the core functionality
  - Includes the `Locator` class and related types

## Key Classes

- [Locator](xref:DbLocator.Locator)
  - The main entry point for the DbLocator library
  - Provides methods for database management, tenant operations, and more

## Getting Started

To use DbLocator in your project:

1. Install the NuGet package
2. Create a new instance of the `Locator` class
3. Use the provided methods to manage your databases

Example:

```csharp
var locator = new Locator(connectionString);
var databases = await locator.GetDatabases();
```

## API Reference

Browse the API documentation using the navigation menu on the left. Each class and method is documented with:

- Detailed descriptions
- Parameter information
- Return value details
- Exception documentation
- Usage examples where applicable 