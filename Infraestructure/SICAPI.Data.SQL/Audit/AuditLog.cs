using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Audit;

public class AuditLog
{
    [Key]
    public long AuditLogId { get; set; }
    public string UserID { get; set; }
    public DateTime EventDateUTC { get; set; }
    public EventType EventType { get; set; }
    public string TypeFullName { get; set; }
    public string RecordId { get; set; }
    public string IP { get; set; } = string.Empty;
    public virtual ICollection<AuditLogDetail> LogDetails { get; set; } = new List<AuditLogDetail>();
}
