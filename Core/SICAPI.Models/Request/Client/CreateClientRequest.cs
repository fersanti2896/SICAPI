namespace SICAPI.Models.Request.Client;

public class CreateClientRequest
{
    public string ContactName { get; set; } = null!;       // Nombre de contacto del cliente
    public string BusinessName { get; set; } = null!;      // Razón social
    public string? Address { get; set; }                   // Dirección
    public string? PhoneNumber { get; set; }               // Teléfono de contacto
    public string? RFC { get; set; }                       // Registro Federal de Contribuyentes
    public string? Email { get; set; }                     // Correo electrónico
    public string? Notes { get; set; }                     // Comentarios adicionales
    public int? PaymentDays { get; set; }                  // Días de pago acordados
    public decimal CreditLimit { get; set; } = 1000;       // Limite de credito
}
