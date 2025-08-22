
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class DetailsNoteCreditResponse : BaseResponse
{
    public List<DetailsNoteCreditDTO>? Result { get; set; }
}
