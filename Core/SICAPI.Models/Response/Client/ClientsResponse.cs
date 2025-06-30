using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Client;

public class ClientsResponse : BaseResponse
{
    public List<ClientDTO>? Result { get; set; }
}
