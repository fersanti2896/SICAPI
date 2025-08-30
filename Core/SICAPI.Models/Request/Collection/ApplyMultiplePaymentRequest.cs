using SICAPI.Models.DTOs;

namespace SICAPI.Models.Request.Collection;

public class ApplyMultiplePaymentRequest
{
    public List<SalePaymentDTO> Sales { get; set; }
    public string Method { get; set; } = null!;
    public int? ThirdPartySupplierId { get; set; }
    public string? Comments { get; set; }
    public DateTime PaymentDate { get; set; }
}
