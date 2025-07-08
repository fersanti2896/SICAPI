using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Catalogs;

public class CPResponse : BaseResponse
{
    public CPDTO? Result { get; set; }
}
