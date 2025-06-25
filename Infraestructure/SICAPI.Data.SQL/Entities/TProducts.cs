using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TProducts")]
public class TProducts : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; } // Identificador del producto (PK)
    public string ProductName { get; set; } // Nombre del producto
    public string? Description { get; set; } // Descripción del producto
    public string Barcode { get; set; } // Código de barras
    public string Presentation { get; set; } // Caja, Frasco, Blíster
    public string? Unit { get; set; } // Unidad de medida (ej. pieza, caja, frasco)
}
