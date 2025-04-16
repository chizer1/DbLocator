namespace DbLocator.Db;

internal class DatabaseUserDatabaseEntity
{
    public int DatabaseUserDatabaseId { get; set; }
    public int DatabaseUserId { get; set; }
    public int DatabaseId { get; set; }
    public virtual DatabaseUserEntity DatabaseUser { get; set; }
    public virtual DatabaseEntity Database { get; set; }
    public virtual ICollection<DatabaseUserEntity> Users { get; set; } = [];
    public virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
