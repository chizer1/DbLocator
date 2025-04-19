using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("DatabaseServer")]
internal class DatabaseServerEntity
{
    public int DatabaseServerId { get; set; }
    public string DatabaseServerName { get; set; }
    public string DatabaseServerHostName { get; set; }
    public string DatabaseServerIpaddress { get; set; }
    public string DatabaseServerFullyQualifiedDomainName { get; set; }
    public bool IsLinkedServer { get; set; }
    public virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
