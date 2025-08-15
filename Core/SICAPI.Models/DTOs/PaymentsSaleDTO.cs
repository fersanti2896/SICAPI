
namespace SICAPI.Models.DTOs;

public class PaymentsSaleDTO
{
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Comments { get; set; }
    public string Username { get; set; }
}
