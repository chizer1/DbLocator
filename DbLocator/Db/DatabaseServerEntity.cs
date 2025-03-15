namespace DbLocator.Db;

internal class DatabaseServerEntity
{
    internal int DatabaseServerId { get; set; }

    internal string DatabaseServerName { get; set; }

    internal string DatabaseServerHostName { get; set; }

    internal string DatabaseServerIpaddress { get; set; }

    internal string DatabaseServerFullyQualifiedDomainName { get; set; }

    internal bool IsLinkedServer { get; set; }

    internal virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
