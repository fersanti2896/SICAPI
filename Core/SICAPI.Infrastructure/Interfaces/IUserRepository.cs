using SICAPI.Models.Request.User;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;

namespace SICAPI.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<ReplyResponse> CreateUser(CreateUserRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
}
