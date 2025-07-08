
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Catalogs;

public class TownByStateAndMunicipalityResponse : BaseResponse
{
    public List<TownDTO>? Result { get; set; }
}
