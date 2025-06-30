using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Supplier;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessSupplier
{
    Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId);
    Task<ReplyResponse> UpdateSupplier(UpdateSupplierRequest request, int userId);
    Task<SuppliersResponse> GetAllSuppliers(int userId);
    Task<ReplyResponse> DeactivateSupplier(ActivateRequest request, int userId);
    Task<EntrySummaryResponse> GetEntryList(int userId);
}
