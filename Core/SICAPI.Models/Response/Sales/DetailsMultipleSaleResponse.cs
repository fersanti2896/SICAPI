

using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class DetailsMultipleSaleResponse : BaseResponse
{
    public List<DetailsMultipleSaleDTO>? Result { get; set; }
}
