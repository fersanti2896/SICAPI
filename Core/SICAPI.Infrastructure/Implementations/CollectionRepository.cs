using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;

namespace SICAPI.Infrastructure.Implementations;

public class CollectionRepository : ICollectionRepository
{
    private readonly IDataAccessCollection IDataAccessCollection;
    private IDataAccessLogs IDataAccessLogs;

    public CollectionRepository(IDataAccessCollection iDataAccessCollection, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessCollection = iDataAccessCollection;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId)
    => await ExecuteWithLogging(() => IDataAccessCollection.ApplyPayment(request, userId), "ApplyPayment", userId);

    public async Task<PaymentStatusResponse> GetAllPaymentStatus(int userId)
    {
        PaymentStatusResponse response = new();
        try
        {
            response = await IDataAccessCollection.GetAllPaymentStatus(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-CollectionRepository",
                Action = "GetAllPaymentStatus",
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
                Module = "SICAPI-CollectionRepository",
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
