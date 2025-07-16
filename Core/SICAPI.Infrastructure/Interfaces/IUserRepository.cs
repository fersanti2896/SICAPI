using SICAPI.Models.Request.User;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;

namespace SICAPI.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<ReplyResponse> CreateUser(CreateUserRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
    Task<UsersReponse> GetAllUsers(int userId);
    Task<ReplyResponse> DeactivateUser(ActivateUserRequest request, int userId);
    Task<UserInfoResponse> CreditInfo(int UserId);
}
