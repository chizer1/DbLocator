namespace DbLocator.Db;

internal class DatabaseUserDatabaseEntity
{
    public int DatabaseUserDatabaseId { get; set; }
    public int DatabaseUserId { get; set; }
    public int DatabaseId { get; set; }
    public virtual DatabaseUserEntity User { get; set; }
    public virtual DatabaseEntity Database { get; set; }
}
