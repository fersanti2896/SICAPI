using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.User;

public class LoginResponse : BaseResponse
{
    public LoginDTO? Result { get; set; }
}
