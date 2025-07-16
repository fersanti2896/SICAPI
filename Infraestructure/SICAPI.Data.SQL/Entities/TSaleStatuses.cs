
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TSaleStatuses")]
public class TSaleStatuses : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SaleStatusId { get; set; }

    public string StatusName { get; set; } = null!; // Nombre del estatus, ej: "En proceso"
}
