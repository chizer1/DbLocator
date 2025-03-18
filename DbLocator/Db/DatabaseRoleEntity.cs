namespace DbLocator.Db;

internal class DatabaseRoleEntity
{
    internal int DatabaseRoleId { get; set; }
    internal string DatabaseRoleName { get; set; }

    internal virtual ICollection<DatabaseUserEntity> Users { get; set; } = [];
}
