using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Request.Warehouse;
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
    Task<MovementsSaleResponse> MovementsSaleBySaleId(DetailsSaleRequest request, int userId);
    Task<SalesResponse> GetSalesByDeliveryId(SaleByStatusRequest request, int userId);
    Task<SalesByUserResponse> GetSalesByUser(SalesByUserRequest request, int userId);
    Task<ReplyResponse> ConfirmReturnAndRevertStock(CancelSaleRequest request, int userId);
    Task<ReplyResponse> CreateCreditNoteRequest(CreditNoteRequest request, int userId);
    Task<ReplyResponse> ConfirmCreditNoteByWarehouse(ConfirmCreditNoteRequest request, int userId);
    Task<DetailsNoteCreditResponse> DetailsNoteCreditById(DetailsNoteCreditRequest request, int userId);
    Task<ReplyResponse> UpdateExpiredSales();
    Task<DetailsMultipleSaleResponse> DetailsMultipleSaleBySaleId(DetailsMultipleSalesRequest request, int userId);
}
