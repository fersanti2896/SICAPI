
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Infrastructure.Implementations;

public class SalesRepository : ISalesRepository
{
    private readonly IDataAccessSales IDataAccessSales;
    private IDataAccessLogs IDataAccessLogs;

    public SalesRepository(IDataAccessSales iDataAccessSales, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessSales = iDataAccessSales;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<ReplyResponse> CreateSale(CreateSaleRequest request, int userId)
    {
        return await ExecuteWithLogging(() => IDataAccessSales.CreateSale(request, userId), "CreateSale", userId);
    }

    public async Task<SalesResponse> GetAllSalesByStatus(SaleByStatusRequest request, int userId)
    {
        SalesResponse response = new();
        try
        {
            response = await IDataAccessSales.GetAllSalesByStatus(request, userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-SalesRepository",
                Action = "GetAllSalesByStatus",
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

    public async Task<DetailsSaleResponse> DetailsSaleBySaleId(DetailsSaleRequest request, int userId)
    {
        return await ExecuteWithLogging(() => IDataAccessSales.DetailsSaleBySaleId(request, userId), "DetailsSaleBySaleId", userId);
    }

    public async Task<SalesStatusResponse> GetAllSalesStatus(int userId)
    {
        SalesStatusResponse response = new();
        try
        {
            response = await IDataAccessSales.GetAllSalesStatus(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-SalesRepository",
                Action = "GetAllSalesStatus",
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

    public async Task<ReplyResponse> AssignDeliveryUser(AssignDeliveryUserRequest request, int userId)
    => await ExecuteWithLogging(() => IDataAccessSales.AssignDeliveryUser(request, userId), "AssignDeliveryUser", userId);

    public async Task<ReplyResponse> UpdateSaleStatus(UpdateSaleStatusRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.UpdateSaleStatus(request, userId), "UpdateSaleStatus", userId);

    public async Task<MovementsSaleResponse> MovementsSaleBySaleId(DetailsSaleRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.MovementsSaleBySaleId(request, userId), "MovementsSaleBySaleId", userId);

    public async Task<SalesResponse> GetSalesByDeliveryId(SaleByStatusRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.GetSalesByDeliveryId(request, userId), "GetSalesByDeliveryId", userId);

    public async Task<SalesByUserResponse> GetSalesByUser(SalesByUserRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.GetSalesByUser(request, userId), "GetSalesByUser", userId);

    public async Task<ReplyResponse> ConfirmReturnAndRevertStock(CancelSaleRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.ConfirmReturnAndRevertStock(request, userId), "ConfirmReturnAndRevertStock", userId);

    public async Task<ReplyResponse> CreateCreditNoteRequest(CreditNoteRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.CreateCreditNoteRequest(request, userId), "CreateCreditNoteRequest", userId);

    public async Task<ReplyResponse> ConfirmCreditNoteByWarehouse(ConfirmCreditNoteRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.ConfirmCreditNoteByWarehouse(request, userId), "ConfirmCreditNoteByWarehouse", userId);

    public async Task<DetailsNoteCreditResponse> DetailsNoteCreditById(DetailsNoteCreditRequest request, int userId)
        => await ExecuteWithLogging(() => IDataAccessSales.DetailsNoteCreditById(request, userId), "DetailsNoteCreditById", userId);

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
                Module = "SICAPI-SalesRepository",
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
