
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class MovementsSaleResponse : BaseResponse
{
    public MovementsSaleDTO? Result { get; set; }
}
