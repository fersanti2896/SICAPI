using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class SalesResponse : BaseResponse
{
    public List<SaleDTO>? Result { get; set; }
}
