using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;


[Table("TEntradaDetalle")]
public class TEntradaDetalle : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntryDetailId { get; set; }
    public int EntryId { get; set; }                        // FK a TEntradasAlmacen
    public int ProductId { get; set; }                     // FK a TProducts
    public int Quantity { get; set; }                       // Cantidad recibida
    public decimal UnitPrice { get; set; }                  // Precio unitario
    public decimal SubTotal { get; set; }                   // Cantidad * Precio
    public DateTime? ExpirationDate { get; set; }
    public string? Lot { get; set; }

    public virtual TEntradasAlmacen? Entry { get; set; }
    public virtual TProducts? Product { get; set; }
}
