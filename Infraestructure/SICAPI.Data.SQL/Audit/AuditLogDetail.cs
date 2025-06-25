using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Audit;

[Table("AuditLogDetail")]
public class AuditLogDetail
{
    [Key]

    public long AuditLogDetailID { get; set; }

    [Required]
    [MaxLength(256)]
    public string PropertyName { get; set; }

    public string OriginalValue { get; set; }

    public string NewValue { get; set; }

    public virtual long AuditLogId { get; set; }

    [ForeignKey("AuditLogId")]
    public virtual AuditLog Log { get; set; }
}
