using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Response;

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
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar proveedor: {ex.Message}"
            };
        }

        return response;
    }
}
