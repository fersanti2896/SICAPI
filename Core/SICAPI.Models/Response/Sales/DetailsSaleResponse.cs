
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class DetailsSaleResponse : BaseResponse
{
    public List<DetailsSaleDTO>? Result { get; set; }
}
