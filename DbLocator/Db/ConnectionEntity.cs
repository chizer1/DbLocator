namespace DbLocator.Db;

internal class ConnectionEntity
{
    internal int ConnectionId { get; set; }

    internal int TenantId { get; set; }

    internal int DatabaseId { get; set; }

    internal virtual DatabaseEntity Database { get; set; }

    internal virtual TenantEntity Tenant { get; set; }
}
