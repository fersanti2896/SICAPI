using SICAPI.Models.Request.Client;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Client;

namespace SICAPI.Infrastructure.Interfaces;

public interface IClientRepository
{
    Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId);
    Task<ReplyResponse> UpdateClient(UpdateClientRequest request, int userId);
    Task<ClientsResponse> GetAllClients(int userId);
    Task<ClientsByUserResponse> GetClientsByUser(int userId);
    Task<ReplyResponse> DeactivateClient(ActivateRequest request, int userId);
    Task<ClientsByUserResponse> GetClientsNotAddressByUser(int userId);
}
