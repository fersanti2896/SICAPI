using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TProductProviders")]
public class TProductProviders : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductProviderId { get; set; }
    public int ProductId { get; set; }                     // ID de nuestro catálogo (FK a TProducts)
    public int SupplierId { get; set; }                    // ID del proveedor (FK a TSuppliers)
    public string ProviderKey { get; set; } = null!;       // Clave del producto con el proveedor
    public string? ProviderDescription { get; set; }       // Nombre o descripción que usa el proveedor
    public decimal UnitPrice { get; set; }                 // Precio de compra por unidad
    public string? Unit { get; set; }                      // Unidad de medida (Ej. Caja, Blíster)

    public virtual TProducts? Product { get; set; }
    public virtual TSuppliers? Supplier { get; set; }
}
