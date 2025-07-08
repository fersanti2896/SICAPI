using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Catalogs;

public class GetStatesResponse : BaseResponse
{
    public List<StatesDTO>? Result { get; set; }
}
