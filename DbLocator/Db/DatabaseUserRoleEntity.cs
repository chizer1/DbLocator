namespace DbLocator.Db;

internal class DatabaseUserRoleEntity
{
    public int DatabaseUserRoleId { get; set; }
    public int DatabaseRoleId { get; set; }
    public int DatabaseUserId { get; set; }
    public virtual DatabaseUserEntity User { get; set; }
    public virtual DatabaseRoleEntity Role { get; set; }
}
