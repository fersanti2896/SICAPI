namespace SICAPI.Models.Request.Client;

public class UpdateClientRequest
{
    public int ClientId { get; set; }                         // ID del cliente
    public string ContactName { get; set; } = null!;
    public string BusinessName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RFC { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public int? PaymentDays { get; set; }
    public decimal CreditLimit { get; set; }
    public string Cve_CodigoPostal { get; set; }           // Clave de CP
    public string Cve_Estado { get; set; }                 // Clave del Estado
    public string Cve_Municipio { get; set; }              // Clave de Municipio
    public string Cve_Colonia { get; set; }                // Clave de la Colonia
    public string Street { get; set; }                     // Calle
    public string ExtNbr { get; set; }                     // Número Ext
    public string InnerNbr { get; set; }                   // Número Int
    public int UserId { get; set; }
}
