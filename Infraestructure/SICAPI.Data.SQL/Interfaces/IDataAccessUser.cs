using SICAPI.Models.Request.User;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessUser
{
    Task<ReplyResponse> CreateUser(CreateUserRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
}
