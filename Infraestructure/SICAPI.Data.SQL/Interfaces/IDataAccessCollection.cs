using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessCollection
{
    Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId);
    Task<PaymentStatusResponse> GetAllPaymentStatus(int userId);
}
