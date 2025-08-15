using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Collection;

public class PaymentsSaleResponse : BaseResponse
{
    public List<PaymentsSaleDTO>? Result { get; set; }
}
