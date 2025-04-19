using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("Database")]
internal class DatabaseEntity
{
    public int DatabaseId { get; set; }
    public string DatabaseName { get; set; }
    public int DatabaseServerId { get; set; }
    public byte DatabaseTypeId { get; set; }
    public byte DatabaseStatusId { get; set; }
    public bool UseTrustedConnection { get; set; }
    public virtual ICollection<ConnectionEntity> Connections { get; set; } = [];
    public virtual ICollection<DatabaseUserDatabaseEntity> Users { get; set; } = [];
    public virtual DatabaseServerEntity DatabaseServer { get; set; }
    public virtual DatabaseTypeEntity DatabaseType { get; set; }
}
