
namespace SICAPI.Models.DTOs;

public class FinanceResumeDTO
{
    public int PaymentId { get; set; }
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
}
