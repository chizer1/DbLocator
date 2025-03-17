namespace DbLocator.Db;

internal class DatabaseUserRoleEntity
{
    internal int DatabaseUserRoleId { get; set; }
    internal int DatabaseRoleId { get; set; }
    internal int DatabaseUserId { get; set; }

    internal virtual ICollection<DatabaseUserEntity> Users { get; set; } = [];
}
