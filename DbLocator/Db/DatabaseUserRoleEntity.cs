namespace DbLocator.Db;

internal class DatabaseUserRoleEntity
{
    internal int DatabaseUserRoleId { get; set; }
    internal int DatabaseRoleId { get; set; }
    internal int DatabaseUserId { get; set; }

    internal virtual DatabaseUserEntity User { get; set; }
    internal virtual DatabaseRoleEntity Role { get; set; }
}
