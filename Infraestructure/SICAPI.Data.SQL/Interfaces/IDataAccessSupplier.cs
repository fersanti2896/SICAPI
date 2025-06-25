using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Response;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessSupplier
{
    Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId);
}
