using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class SalesPendingPaymentResponse : BaseResponse
{
    public List<SalesPendingPaymentDTO>? Result { get; set; }
}
