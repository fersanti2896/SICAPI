using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Client;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Client;
using SICAPI.Shared.Helpers;

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
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            var entity = new TClients
            {
                UserId = request.UserId,
                ClientName = TextHelper.CapitalizeEachWord(request.ContactName),
                BusinessName = TextHelper.CapitalizeEachWord(request.BusinessName),
                Phone = request.PhoneNumber,
                Email = request.Email,
                RFC = request.RFC,
                CreditLimit = request.CreditLimit,
                PaymentDays = request.PaymentDays,
                Notes = TextHelper.Capitalize(request.Notes),
                IsBlocked = 0,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            Context.TClients.Add(entity);
            await Context.SaveChangesAsync();

            var address = new TClientsAddress
            {
                ClientId = entity.ClientId,
                Cve_CodigoPostal = request.Cve_CodigoPostal,
                Cve_Estado = request.Cve_Estado,
                Cve_Municipio = request.Cve_Municipio,
                Cve_Colonia = request.Cve_Colonia,
                Street = TextHelper.CapitalizeEachWord(request.Street),
                ExtNbr = request.ExtNbr,
                InnerNbr = request.InnerNbr,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            Context.TClientsAddress.Add(address);
            await Context.SaveChangesAsync();

            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Cliente registrado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

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

        using var transaction = await Context.Database.BeginTransactionAsync();

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

            // Actualizar datos del cliente
            client.UserId = request.UserId;
            client.ClientName = TextHelper.CapitalizeEachWord(request.ContactName);
            client.BusinessName = TextHelper.CapitalizeEachWord(request.BusinessName);
            client.Phone = request.PhoneNumber;
            client.RFC = request.RFC;
            client.Email = request.Email;
            client.Notes = TextHelper.Capitalize(request.Notes);
            client.PaymentDays = request.PaymentDays;
            client.CreditLimit = request.CreditLimit;
            client.UpdateDate = DateTime.Now;
            client.UpdateUser = userId;

            // Buscar dirección
            var address = await Context.TClientsAddress.FirstOrDefaultAsync(a => a.ClientId == request.ClientId);

            if (address != null)
            {
                // Actualizar dirección existente
                address.Cve_CodigoPostal = request.Cve_CodigoPostal;
                address.Cve_Estado = request.Cve_Estado;
                address.Cve_Municipio = request.Cve_Municipio;
                address.Cve_Colonia = request.Cve_Colonia;
                address.Street = TextHelper.CapitalizeEachWord(request.Street);
                address.ExtNbr = request.ExtNbr;
                address.InnerNbr = request.InnerNbr;
                address.UpdateDate = DateTime.Now;
                address.UpdateUser = userId;
            }
            else
            {
                // Crear nueva dirección si no existe
                var newAddress = new TClientsAddress
                {
                    ClientId = request.ClientId,
                    Cve_CodigoPostal = request.Cve_CodigoPostal,
                    Cve_Estado = request.Cve_Estado,
                    Cve_Municipio = request.Cve_Municipio,
                    Cve_Colonia = request.Cve_Colonia,
                    Street = TextHelper.CapitalizeEachWord(request.Street),
                    ExtNbr = request.ExtNbr,
                    InnerNbr = request.InnerNbr,
                    CreateDate = DateTime.Now,
                    CreateUser = userId,
                    Status = 1
                };
                await Context.TClientsAddress.AddAsync(newAddress);
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Cliente y dirección actualizados correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

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

    public async Task<ClientsResponse> GetAllClients(int userId)
    {
        ClientsResponse response = new();

        try
        {
            var clients = await (from c in Context.TClients
                                 join ca in Context.TClientsAddress on c.ClientId equals ca.ClientId into addr
                                 from ca in addr.DefaultIfEmpty()
                                 join pc in Context.TPostalCodes on
                                    new { ca.Cve_CodigoPostal, ca.Cve_Estado, ca.Cve_Municipio, ca.Cve_Colonia }
                                    equals new
                                    {
                                        Cve_CodigoPostal = pc.d_codigo,
                                        Cve_Estado = pc.c_estado,
                                        Cve_Municipio = pc.c_mnpio,
                                        Cve_Colonia = pc.id_asenta_cpcons
                                    } into postal
                                 from pc in postal.DefaultIfEmpty()
                                 select new ClientDTO
                                 {
                                     ClientId = c.ClientId,
                                     ContactName = c.ClientName,
                                     BusinessName = c.BusinessName,
                                     CreditLimit = c.CreditLimit,
                                     Email = c.Email,
                                     PhoneNumber = c.Phone,
                                     RFC = c.RFC,
                                     Address = ca != null
                                         ? $"{ca.Street} #{ca.ExtNbr}, {pc.d_asenta}, {pc.D_mnpio}, {pc.d_estado}, CP {pc.d_codigo}"
                                         : null,
                                     Notes = c.Notes,
                                     PaymentDays = c.PaymentDays,
                                     Cve_CodigoPostal = ca.Cve_CodigoPostal,
                                     Cve_Estado = ca.Cve_Estado,
                                     Cve_Municipio = ca.Cve_Municipio,
                                     Cve_Colonia = ca.Cve_Colonia,
                                     Street = ca.Street,
                                     ExtNbr = ca.ExtNbr,
                                     InnerNbr = ca.InnerNbr,
                                     UserId = c.UserId,
                                     Status = c.Status,
                                     DescriptionStatus = c.Status == 1 ? "Activo" : "Inactivo"
                                 }).ToListAsync();

            response.Result = clients;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessClient",
                Action = "GetAllClients",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error Exception: {ex.InnerException?.Message ?? ex.Message}"
            };
        }

        return response;
    }

    public async Task<ReplyResponse> DeactivateClient(ActivateRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var client = await Context.TClients.FirstOrDefaultAsync(u => u.ClientId == request.Id);

            if (client == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Cliente no encontrado"
                };
                return response;
            }

            client.Status = request.Status;
            client.UpdateDate = DateTime.Now;
            client.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = request.Status == 1 ? "Cliente activado correctamente" : "Cliente desactivado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessClient",
                Action = "DeactivateClient",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al desactivar cliente: {ex.Message}"
            };
        }

        return response;
    }
}
