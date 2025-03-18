namespace DbLocator.Db;

internal class DatabaseUserEntity
{
    internal int DatabaseUserId { get; set; }
    internal int DatabaseId { get; set; }
    internal string UserName { get; set; }
    internal string UserPassword { get; set; }

    internal virtual DatabaseEntity Database { get; set; }

    internal virtual ICollection<DatabaseRoleEntity> UserRoles { get; set; } = [];
}
