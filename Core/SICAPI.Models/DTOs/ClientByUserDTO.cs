
namespace SICAPI.Models.DTOs;

public class ClientByUserDTO
{
    public int ClientId { get; set; }
    public string ContactName { get; set; } = null!;       // Nombre de contacto del cliente
    public string BusinessName { get; set; } = null!;      // Razón social
    public decimal CreditLimit { get; set; }               // Limite de credito
    public decimal AvailableCredit { get; set; }           // Crédito Disponible
    public int? PaymentDays { get; set; }
    public int IsBlocked { get; set; }
}
