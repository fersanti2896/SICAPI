using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TSuppliers")]
public class TSuppliers : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SupplierId { get; set; }
    public string BusinessName { get; set; } // Razón social
    public string? ContactName { get; set; } // Persona de contacto
    public string? Phone { get; set; } // Teléfono
    public string? Email { get; set; } // Correo electrónico
    public string? RFC { get; set; } // Registro Federal de Contribuyentes
    public string? Address { get; set; } // Dirección
    public string? PaymentTerms { get; set; } // Condiciones de pago
    public string? Notes { get; set; } // Notas adicionales
    public decimal? ThirdPartyBalance { get; set; } // Saldo Disponible (Para Cuenta de Tercero)
}
