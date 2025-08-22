using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Finance;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;
using SICAPI.Models.Response.Finance;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessCollection
{
    Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId);
    Task<PaymentStatusResponse> GetAllPaymentStatus(int userId);
    Task<SalesPendingPaymentResponse> GetSalesPendingPayment(SalesPendingPaymentRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesHistorical(SalesHistoricalRequest request, int userId);
    Task<SalesPendingPaymentResponse> GetSalesPaids(SalesHistoricalRequest request, int userId);
    Task<ReplyResponse> CancelSaleWithComment(CancelSaleRequest request, int userId);
    Task<CancelledSaleCommentResponse> GetCancelledSaleComments(CancelledCommentsRequest request, int userId);
    Task<ReplyResponse> CancelSaleByOmission(CancelSaleRequest request, int userId);
    Task<FinanceBuildResponse> GetFinanceSummaryAsync(FinanceBuildRequest request, int userId);
    Task<ReplyResponse> ApplyMultiplePayments(ApplyMultiplePaymentRequest request, int userId);
    Task<PaymentsSaleResponse> PaymentsSaleBySaleId(DetailsSaleRequest request, int userId);
    Task<CreditNoteListResponse> GetCreditNotesByStatus(CreditNoteListRequest request, int userId);
    Task<ReplyResponse> ApproveCreditNoteByCollection(ApproveCreditNoteRequest request, int userId);
}
