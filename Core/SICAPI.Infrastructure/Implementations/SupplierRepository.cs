using SICAPI.Data.SQL.Implementations;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;

namespace SICAPI.Infrastructure.Implementations;

public class SupplierRepository : ISupplierRepository
{
    private readonly IDataAccessSupplier IDataAccessSupplier;
    private IDataAccessLogs IDataAccessLogs;

    public SupplierRepository(IDataAccessSupplier iDataAccessSupplier, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessSupplier = iDataAccessSupplier;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            response = await IDataAccessSupplier.CreateSupplier(request, userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-SupplierRepository",
                Action = "CreateSupplier",
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
}
