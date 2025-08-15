

namespace SICAPI.Models.Request.Sales;

public class ApplyPaymentRequest
{
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
    public string? Comments { get; set; }
    public DateTime? PaymentDate { get; set; }
}
