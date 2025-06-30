using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.User;

public class UsersReponse : BaseResponse
{
    public List<UserDTO>? Result { get; set; }
}
