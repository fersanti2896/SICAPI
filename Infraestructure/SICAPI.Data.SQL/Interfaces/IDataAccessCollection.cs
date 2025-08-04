using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessCollection
{
    Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId);
    Task<PaymentStatusResponse> GetAllPaymentStatus(int userId);
    Task<SalesPendingPaymentResponse> GetSalesPendingPayment(SalesPendingPaymentRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesHistorical(SalesHistoricalRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesPaids(SalesHistoricalRequest request, int userId);
}
