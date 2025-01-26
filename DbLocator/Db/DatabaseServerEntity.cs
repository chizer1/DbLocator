namespace DbLocator.Db;

internal class DatabaseServerEntity
{
    public int DatabaseServerId { get; set; }

    public string DatabaseServerName { get; set; }

    public string DatabaseServerIpaddress { get; set; }

    public virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
