
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TClients")]
public class TClients : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public string ClientName { get; set; } = null!;       // Nombre del cliente o contacto
    public string? BusinessName { get; set; }             // Razón social
    public string? Phone { get; set; }                    // Teléfono de contacto
    public string? Email { get; set; }                    // Correo electrónico
    public string? RFC { get; set; }                      // RFC del cliente
    public decimal CreditLimit { get; set; }              // Límite de crédito asignado
    public int? PaymentDays { get; set; }                 // Días de crédito permitidos
    public string? Notes { get; set; }                    // Comentarios u observaciones
    public int IsBlocked { get; set; } = 0;               // Bloqueado para ventas

    public virtual TUsers? User { get; set; }
}
