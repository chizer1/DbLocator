using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("DatabaseType")]
internal class DatabaseTypeEntity
{
    public byte DatabaseTypeId { get; set; }
    public string DatabaseTypeName { get; set; }
    public virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
