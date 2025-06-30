using SICAPI.Models.Request.Client;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Client;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessClient
{
    Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId);
    Task<ReplyResponse> UpdateClient(UpdateClientRequest request, int userId);
    Task<ReplyResponse> ChangeClientUser(UpdateClientUserRequest request, int userId);
    Task<ClientsResponse> GetAllClients(int userId);
}
