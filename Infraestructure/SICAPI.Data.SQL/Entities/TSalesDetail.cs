using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Entities;

[Table("TSalesDetail")]
public class TSalesDetail : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SaleDetailId { get; set; }
    public int SaleId { get; set; }                  // ID de la venta
    public int ProductId { get; set; }               // ID del producto
    public int Quantity { get; set; }                // Cantidad vendida
    public decimal UnitPrice { get; set; }           // Precio al que se vendió el producto
    public decimal SubTotal { get; set; }            // Subtotal = Cantidad * Precio unitario
    public string? Lot { get; set; }                 // Número de lote del producto
    public DateTime? ExpirationDate { get; set; }    // Fecha de caducidad del producto (si aplica)


    [ForeignKey("SaleId")]
    public virtual TSales? Sale { get; set; }

    [ForeignKey("ProductId")]
    public virtual TProducts? Product { get; set; }
}
