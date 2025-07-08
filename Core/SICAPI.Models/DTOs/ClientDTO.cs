namespace SICAPI.Models.DTOs;

public class ClientDTO
{
    public int ClientId { get; set; }
    public string ContactName { get; set; } = null!;       // Nombre de contacto del cliente
    public string BusinessName { get; set; } = null!;      // Razón social
    public string? Address { get; set; }                   // Dirección
    public string? PhoneNumber { get; set; }               // Teléfono de contacto
    public string? RFC { get; set; }                       // Registro Federal de Contribuyentes
    public string? Email { get; set; }                     // Correo electrónico
    public decimal CreditLimit { get; set; } = 1000;       // Limite de credito
    public string? Notes { get; set; }                     // Comentarios adicionales
    public int? PaymentDays { get; set; }                  // Días de pago acordados
    public string Cve_CodigoPostal { get; set; }           // Clave de CP
    public string Cve_Estado { get; set; }                 // Clave del Estado
    public string Cve_Municipio { get; set; }              // Clave de Municipio
    public string Cve_Colonia { get; set; }                // Clave de la Colonia
    public string Street { get; set; }                     // Calle
    public string ExtNbr { get; set; }                     // Número Ext
    public string InnerNbr { get; set; }                   // Número Int
    public int UserId { get; set; }
    public int Status { get; set; }
    public string DescriptionStatus { get; set; }
}
