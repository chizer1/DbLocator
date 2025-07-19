---
_layout: landing
---

# DbLocator Documentation

Welcome to the DbLocator documentation. This documentation will help you understand and use the DbLocator library effectively.

## What is DbLocator?

DbLocator is a .NET library that simplifies database interactions for multi-database tenant applications on SQL Server. It provides a robust foundation for managing connections, roles, and sharding logic—all with built-in security features.

1. Multi-Tenant Database Management
Seamlessly manage isolated databases for each tenant—ideal for SaaS platforms needing data separation and centralized control.

2. Database Sharding Across Servers
Scale horizontally by distributing tenant databases across multiple SQL Server instances, identified by hostname, FQDN, or IP address.

3. Role-Based Access Control
Enforce fine-grained user permissions by integrating with SQL Server roles. Easily map application roles to database roles.

4. Secure Connection Handling
Supports SQL Server authentication with options for trusted or username/password connections, including database user provisioning if desired.

5. Data Encryption
Encrypt and decrypt credentials or other secure metadata using a custom key, ensuring compliance and security.

6. Connection Caching for Performance
Integrate custom or built-in caching to avoid repeated resolution of tenant configurations, improving performance under load.

## Getting Started

To get started with DbLocator, check out [Getting Started Guide](articles/getting-started.md).

## Examples

Check out the [Examples](articles/examples.md) to see DbLocator in action.

## API Reference

For detailed API documentation, visit [API Reference](/DbLocator/api/DbLocator).

## Contributing

I welcome contributions! Please see [Contributing Guide](https://github.com/chizer1/DbLocator/blob/master/CONTRIBUTING.md) for more information.

## License

DbLocator is licensed under the MIT License. See the [LICENSE](https://github.com/chizer1/DbLocator/blob/master/LICENSE) file for details.

## Support

If you need help or have questions:

- Open an [issue](https://github.com/chizer1/DbLocator/issues)
- Check our [documentation](articles/getting-started.md)
- Join our [discussions](https://github.com/chizer1/DbLocator/discussions)
