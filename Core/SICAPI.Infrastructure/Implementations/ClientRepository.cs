using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Client;
using SICAPI.Models.Response;

namespace SICAPI.Infrastructure.Implementations;

public class ClientRepository : IClientRepository
{
    private readonly IDataAccessClient IDataAccessClient;
    private IDataAccessLogs IDataAccessLogs;

    public ClientRepository(IDataAccessClient iDataAccessClient, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessClient = iDataAccessClient;
        IDataAccessLogs = iDataAccessLogs;
    }

    public Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessClient.CreateClient(request, userId), "CreateClient", userId);
    }

    public Task<ReplyResponse> UpdateClient(UpdateClientRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessClient.UpdateClient(request, userId), "UpdateClient", userId);
    }
    public Task<ReplyResponse> ChangeClientUser(UpdateClientUserRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessClient.ChangeClientUser(request, userId), "ChangeClientUser", userId);
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
                Module = "SICAPI-ClientRepository",
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
