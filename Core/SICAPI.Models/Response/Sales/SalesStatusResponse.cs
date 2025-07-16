
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class SalesStatusResponse : BaseResponse
{
    public List<SalesStatusDTO>? Result { get; set; }
}
