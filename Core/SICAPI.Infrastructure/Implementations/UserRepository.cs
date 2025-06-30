using Azure.Core;
using SICAPI.Data.SQL.Implementations;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.User;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;

namespace SICAPI.Infrastructure.Implementations;

public class UserRepository : IUserRepository
{
    private readonly IDataAccessUser IDataAccessUser;
    private IDataAccessLogs IDataAccessLogs;

    public UserRepository(IDataAccessUser iDataAccessUser, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessUser = iDataAccessUser;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<ReplyResponse> CreateUser(CreateUserRequest request)
    {
        ReplyResponse response = new();

        try
        {
            response = await IDataAccessUser.CreateUser(request);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-UserRepository",
                Action = "CreateUser",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        LoginResponse response = new();

        try
        {
            response = await IDataAccessUser.Login(request);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-UserRepository",
                Action = "Login",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
    {
        LoginResponse response = new();

        try
        {
            response = await IDataAccessUser.RefreshToken(request);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-UserRepository",
                Action = "RefreshToken",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public Task<ReplyResponse> DeactivateUser(ActivateUserRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessUser.DeactivateUser(request, userId), "DeactivateUser", userId);
    }

    public async Task<UsersReponse> GetAllUsers(int userId)
    {
        UsersReponse response = new();
        try
        {
            response = await IDataAccessUser.GetAllUsers(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-UserRepository",
                Action = "GetAllUsers",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };

            return response;
        }
    }

    private async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string actionName, int userId) where T : BaseResponse, new()
    {
        T response = new();

        try
        {
            response = await action();
            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-UserRepository",
                Action = actionName,
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
            return response;
        }
    }
}
