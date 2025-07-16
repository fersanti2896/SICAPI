using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Warehouse;

public class StockRealResponse : BaseResponse
{
    public List<StockRealDTO>? Result { get; set; }
}
