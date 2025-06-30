using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Client;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Client;
using SICAPI.Models.Response.Warehouse;
using System.Net;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessClient : IDataAccessClient
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }

    public DataAccessClient(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var entity = new TClients
            {
                UserId = userId,
                ClientName = request.ContactName,
                BusinessName = request.BusinessName,
                Address = request.Address,
                Phone = request.PhoneNumber,
                Email = request.Email,
                RFC = request.RFC,
                CreditLimit = request.CreditLimit,
                PaymentDays = request.PaymentDays,
                Notes = request.Notes,
                IsBlocked = 0,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            Context.TClients.Add(entity);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Cliente registrado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessClient",
                Action = "CreateClient",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar cliente: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<ReplyResponse> UpdateClient(UpdateClientRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var client = await Context.TClients.FindAsync(request.ClientId);

            if (client == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Cliente no encontrado."
                };
                return response;
            }

            client.ClientName = request.ContactName;
            client.BusinessName = request.BusinessName;
            client.Address = request.Address;
            client.Phone = request.PhoneNumber;
            client.RFC = request.RFC;
            client.Email = request.Email;
            client.Notes = request.Notes;
            client.PaymentDays = request.PaymentDays;
            client.CreditLimit = request.CreditLimit;
            client.UpdateDate = DateTime.Now;
            client.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Cliente actualizado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessClient",
                Action = "UpdateClient",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al actualizar cliente: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<ReplyResponse> ChangeClientUser(UpdateClientUserRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var client = await Context.TClients.FindAsync(request.ClientId);

            if (client == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Cliente no encontrado."
                };
                return response;
            }

            client.UserId = request.NewUserId;
            client.UpdateDate = DateTime.Now;
            client.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Usuario asignado al cliente actualizado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessClient",
                Action = "ChangeClientUser",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al cambiar el usuario del cliente: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<ClientsResponse> GetAllClients(int userId)
    {
        ClientsResponse response = new();

        try
        {
            var clients = await Context.TClients
                                     .Select(u => new ClientDTO
                                     {
                                        ClientId = u.ClientId,
                                        ContactName = u.ClientName,
                                        BusinessName = u.BusinessName,
                                        Address = u.Address,
                                        CreditLimit = u.CreditLimit,    
                                        Email = u.Email,
                                        PhoneNumber = u.Phone,
                                        RFC = u.RFC
                                     })
                                    .ToListAsync();

            response.Result = clients;

            return response;
        }
        catch (Exception ex)
        {
            return new ClientsResponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }
    }
}
