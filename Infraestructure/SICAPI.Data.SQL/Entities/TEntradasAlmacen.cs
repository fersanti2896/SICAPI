
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TEntradasAlmacen")]
public class TEntradasAlmacen : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntryId { get; set; }
    public int SupplierId { get; set; } // Proveedor que entrega el pedido
    public string InvoiceNumber { get; set; } = string.Empty; // Número de factura o remisión
    public DateTime EntryDate { get; set; } // Fecha de entrada
    public DateTime? ExpectedPaymentDate { get; set; } // Fecha pactada para pago
    public decimal TotalAmount { get; set; } // Monto total de la compra
    public string? Observations { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? AmountPending { get; set; }

    public virtual TSuppliers? Supplier { get; set; }
}
