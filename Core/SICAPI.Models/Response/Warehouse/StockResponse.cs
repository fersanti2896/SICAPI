using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Warehouse;

public class StockResponse : BaseResponse
{
    public List<StockDTO>? Result { get; set; }
}
