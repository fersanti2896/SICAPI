using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TInventory")]
public class TInventory : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InventoryId { get; set; }
    public int ProductId { get; set; }                         // Producto en catálogo
    public int CurrentStock { get; set; }                      // Cantidad disponible actual
    public int? MinStock { get; set; }                         // Nivel mínimo (para alertas)
    public int? MaxStock { get; set; }                         // Nivel máximo recomendado
    public DateTime? LastEntryDate { get; set; }               // Última vez que se recibió stock
    public DateTime? LastUpdateDate { get; set; }              // Última modificación de inventario
    public int? Apartado { get; set; }                         // Producto que se va ir apartando por una venta
    public int? StockReal { get; set; }                        // Stock Real o en Tiempo Real

    public virtual TProducts? Product { get; set; }
}
