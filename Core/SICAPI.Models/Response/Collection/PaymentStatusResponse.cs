using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Collection;

public class PaymentStatusResponse : BaseResponse
{
    public List<PaymentStatusDTO>? Result { get; set; }
}
