using SICAPI.Data.SQL.Implementations;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Supplier;
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

    public Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessSupplier.CreateSupplier(request, userId), "CreateSupplier", userId);
    }


    public Task<ReplyResponse> UpdateSupplier(UpdateSupplierRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessSupplier.UpdateSupplier(request, userId), "UpdateSupplier", userId);
    }

    public async Task<SuppliersResponse> GetAllSuppliers(int userId)
    {
        SuppliersResponse response = new();
        try
        {
            response = await IDataAccessSupplier.GetAllSuppliers(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-SupplierRepository",
                Action = "GetAllSuppliers",
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

    public async Task<EntrySummaryResponse> GetEntryList(int userId)
    {
        EntrySummaryResponse response = new();
        try
        {
            response = await IDataAccessSupplier.GetEntryList(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-SupplierRepository",
                Action = "GetEntryList",
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

    public Task<ReplyResponse> DeactivateSupplier(ActivateRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessSupplier.DeactivateSupplier(request, userId), "DeactivateUser", userId);
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
                Module = "SICAPI-SupplierRepository",
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
