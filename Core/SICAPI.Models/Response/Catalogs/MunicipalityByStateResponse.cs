
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Catalogs;

public class MunicipalityByStateResponse : BaseResponse
{
    public List<MunicipalityDTO>? Result { get; set; }
}
