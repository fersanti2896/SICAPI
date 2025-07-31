using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessSales
{
    Task<ReplyResponse> CreateSale(CreateSaleRequest request, int userId);
    Task<SalesResponse> GetAllSalesByStatus(SaleByStatusRequest request, int UseruserIdId);
    Task<DetailsSaleResponse> DetailsSaleBySaleId(DetailsSaleRequest request, int UserId);
    Task<SalesStatusResponse> GetAllSalesStatus(int userId);
    Task<ReplyResponse> AssignDeliveryUser(AssignDeliveryUserRequest request, int userId);
    Task<ReplyResponse> UpdateSaleStatus(UpdateSaleStatusRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesPendingPayment(int userId);
    Task<MovementsSaleResponse> MovementsSaleBySaleId(DetailsSaleRequest request, int userId);
    Task<SalesResponse> GetSalesByDeliveryId(SaleByStatusRequest request, int userId);
    Task<SalesByUserResponse> GetSalesByUser(SalesByUserRequest request, int userId);
}
