using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Entities;

[Table("TLogs")]
public class TLog : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? Message { get; set; }
    public string? InnerException { get; set; }
}
