using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("DatabaseUser")]
internal class DatabaseUserEntity
{
    public int DatabaseUserId { get; set; }
    public string UserName { get; set; }
    public string UserPassword { get; set; }
    public virtual ICollection<DatabaseUserRoleEntity> UserRoles { get; set; } = [];
    public virtual ICollection<DatabaseUserDatabaseEntity> Databases { get; set; } = [];
}
