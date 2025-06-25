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
}
