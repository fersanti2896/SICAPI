
namespace SICAPI.Models.Request.Finance;

public class FinanceResumeRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? PaymentMethod { get; set; }
}
