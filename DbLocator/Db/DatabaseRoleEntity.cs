using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("DatabaseRole")]
internal class DatabaseRoleEntity
{
    public int DatabaseRoleId { get; set; }
    public string DatabaseRoleName { get; set; }
    public virtual ICollection<DatabaseUserRoleEntity> Users { get; set; } = [];
}
