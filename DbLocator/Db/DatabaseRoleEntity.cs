namespace DbLocator.Db;

internal class DatabaseRoleEntity
{
    public int DatabaseRoleId { get; set; }
    public string DatabaseRoleName { get; set; }
    public virtual ICollection<DatabaseUserRoleEntity> Users { get; set; } = [];
}
