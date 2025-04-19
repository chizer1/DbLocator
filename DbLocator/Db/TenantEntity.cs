using System.ComponentModel.DataAnnotations.Schema;

namespace DbLocator.Db;

[Table("Tenant")]
internal class TenantEntity
{
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string TenantCode { get; set; }
    public byte TenantStatusId { get; set; }
    public virtual ICollection<ConnectionEntity> Connections { get; set; } = [];
}
