namespace DbLocator.Db;

internal class ConnectionEntity
{
    public int ConnectionId { get; set; }
    public int TenantId { get; set; }
    public int DatabaseId { get; set; }
    public virtual DatabaseEntity Database { get; set; }
    public virtual TenantEntity Tenant { get; set; }
}
