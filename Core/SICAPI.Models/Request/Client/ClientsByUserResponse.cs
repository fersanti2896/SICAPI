using SICAPI.Models.DTOs;
using SICAPI.Models.Response;

namespace SICAPI.Models.Request.Client;

public class ClientsByUserResponse : BaseResponse
{
    public List<ClientByUserDTO>? Result { get; set; }
}
