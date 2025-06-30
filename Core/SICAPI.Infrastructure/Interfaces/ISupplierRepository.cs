using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Supplier;

namespace SICAPI.Infrastructure.Interfaces;

public interface ISupplierRepository
{
    Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId);
    Task<ReplyResponse> UpdateSupplier(UpdateSupplierRequest request, int userId);
    Task<SuppliersResponse> GetAllSuppliers(int userId);
    Task<EntrySummaryResponse> GetEntryList(int userId);
    Task<ReplyResponse> DeactivateSupplier(ActivateRequest request, int userId);
}
