using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Supplier;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessSupplier : IDataAccessSupplier
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }

    public DataAccessSupplier(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var supplier = new TSuppliers
            {
                BusinessName = request.BusinessName,
                ContactName = request.ContactName,
                Phone = request.Phone,
                Email = request.Email,
                RFC = request.RFC,
                Address = request.Address,
                PaymentTerms = request.PaymentTerms,
                Notes = request.Notes,
                Status = 1,
                CreateDate = DateTime.Now,
                CreateUser = userId
            };

            Context.TSuppliers.Add(supplier);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Proveedor registrado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSupplier",
                Action = "CreateSupplier",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar proveedor: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<ReplyResponse> UpdateSupplier(UpdateSupplierRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var supplier = await Context.TSuppliers.FirstOrDefaultAsync(s => s.SupplierId == request.SupplierId);

            if (supplier == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Proveedor no encontrado."
                };
                return response;
            }

            supplier.BusinessName = request.BusinessName;
            supplier.ContactName = request.ContactName;
            supplier.Phone = request.Phone;
            supplier.Email = request.Email;
            supplier.RFC = request.RFC;
            supplier.Address = request.Address;
            supplier.PaymentTerms = request.PaymentTerms;
            supplier.Notes = request.Notes;
            supplier.UpdateDate = DateTime.Now;
            supplier.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Proveedor actualizado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSupplier",
                Action = "UpdateSupplier",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al actualizar proveedor: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<SuppliersResponse> GetAllSuppliers(int userId)
    {
        SuppliersResponse response = new();

        try
        {
            var users = await Context.TSuppliers
                                     .Select(u => new SupplierDTO
                                     {
                                        SupplierId = u.SupplierId,
                                        BusinessName = u.BusinessName,
                                        ContactName = u.ContactName,
                                        Phone = u.Phone,
                                        Address = u.Address,
                                        Status = u.Status,
                                        DescriptionStatus = u.Status == 1 ? "Activo" : "Inactivo"
                                     })
                                    .ToListAsync();

            response.Result = users;

            return response;
        }
        catch (Exception ex)
        {
            return new SuppliersResponse
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

    public async Task<ReplyResponse> DeactivateSupplier(ActivateRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var supplier = await Context.TSuppliers.FirstOrDefaultAsync(u => u.SupplierId == request.Id);

            if (supplier == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Proveedor no encontrado"
                };
                return response;
            }

            supplier.Status = request.Status;
            supplier.UpdateDate = DateTime.Now;
            supplier.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = request.Status == 1 ? "Proveedor activado correctamente" : "Proveedor desactivado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al desactivar proveedor: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<EntrySummaryResponse> GetEntryList(int userId)
    {
        EntrySummaryResponse response = new();

        try
        {
            var entries = await Context.TEntradasAlmacen
                                       .Include(e => e.Supplier)
                                       .Select(e => new EntrySummaryDTO
                                       {
                                        SupplierId = e.SupplierId,
                                        BusinessName = e.Supplier!.BusinessName,
                                        InvoiceNumber = e.InvoiceNumber,
                                        EntryDate = e.EntryDate,
                                        ExpectedPaymentDate = e.ExpectedPaymentDate,
                                        TotalAmount = e.TotalAmount
                                       })
                                       .ToListAsync();

            response.Result = entries;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener listado de entradas: {ex.Message}"
            };
        }

        return response;
    }
}
