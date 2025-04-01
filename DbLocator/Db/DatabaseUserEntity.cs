namespace DbLocator.Db;

internal class DatabaseUserEntity
{
    public int DatabaseUserId { get; set; }
    public int DatabaseId { get; set; }
    public string UserName { get; set; }
    public string UserPassword { get; set; }
    public virtual DatabaseEntity Database { get; set; }
    public virtual ICollection<DatabaseUserRoleEntity> UserRoles { get; set; } = [];
}
