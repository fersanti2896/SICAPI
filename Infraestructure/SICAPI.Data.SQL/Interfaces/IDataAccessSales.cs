using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessSales
{
    Task<ReplyResponse> CreateSale(CreateSaleRequest request, int userId);
    Task<SalesResponse> GetAllSalesByStatus(SaleByStatusRequest request, int UserId);
    Task<DetailsSaleResponse> DetailsSaleBySaleId(DetailsSaleRequest request, int UserId);
    Task<SalesStatusResponse> GetAllSalesStatus(int UserId);
    Task<ReplyResponse> AssignDeliveryUser(AssignDeliveryUserRequest request, int userId);
    Task<ReplyResponse> UpdateSaleStatus(UpdateSaleStatusRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesPendingPayment(int userId);
    Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId);
}
