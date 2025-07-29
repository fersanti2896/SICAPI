using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class SalesByUserResponse : BaseResponse
{
    public List<SalesByUserDTO>? Result { get; set; }
}
