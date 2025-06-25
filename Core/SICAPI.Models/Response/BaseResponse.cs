using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response;

public class BaseResponse
{
    public ErrorDTO? Error { get; set; }
}
