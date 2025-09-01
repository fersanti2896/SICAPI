using SICAPI.Models.Request.Client;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Client;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessClient
{
    Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId);
    Task<ReplyResponse> UpdateClient(UpdateClientRequest request, int userId);
    Task<ClientsResponse> GetAllClients(int userId);
    Task<ReplyResponse> DeactivateClient(ActivateRequest request, int userId);
    Task<ClientsByUserResponse> GetClientsByUser(int userId);
    Task<ClientsByUserResponse> GetClientsNotAddressByUser(int userId);
}
