using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Finance;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;
using SICAPI.Models.Response.Finance;
using SICAPI.Models.Response.Sales;

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

    public async Task<SalesPendingPaymentResponse> GetSalesPendingPayment(SalesPendingPaymentRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.GetSalesPendingPayment(request, userId), "GetSalesPendingPayment", userId);

    public async Task<SalesPendingPaymentResponse> GetSalesHistorical(SalesHistoricalRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.GetSalesHistorical(request, userId), "GetSalesHistorical", userId);

    public async Task<SalesPendingPaymentResponse> GetSalesPaids(SalesHistoricalRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.GetSalesPaids(request, userId), "GetSalesPaids", userId);

    public async Task<ReplyResponse> CancelSaleWithComment(CancelSaleRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.CancelSaleWithComment(request, userId), "CancelSaleWithCommentAsync", userId);

    public async Task<CancelledSaleCommentResponse> GetCancelledSaleComments(CancelledCommentsRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.GetCancelledSaleComments(request, userId), "GetCancelledSaleComments", userId);

    public async Task<ReplyResponse> CancelSaleByOmission(CancelSaleRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.CancelSaleByOmission(request, userId), "CancelSaleByOmission", userId);

    public async Task<FinanceBuildResponse> GetFinanceSummaryAsync(FinanceBuildRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessCollection.GetFinanceSummaryAsync(request, userId), "GetFinanceSummaryAsync", userId);

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
