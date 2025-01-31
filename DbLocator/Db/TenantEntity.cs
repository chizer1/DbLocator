namespace DbLocator.Db;

internal class TenantEntity
{
    internal int TenantId { get; set; }

    internal string TenantName { get; set; }

    internal string TenantCode { get; set; }

    internal byte TenantStatusId { get; set; }

    internal virtual ICollection<ConnectionEntity> Connections { get; set; } = [];
}
