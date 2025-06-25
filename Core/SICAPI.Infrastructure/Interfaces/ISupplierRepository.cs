using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Response;

namespace SICAPI.Infrastructure.Interfaces;

public interface ISupplierRepository
{
    Task<ReplyResponse> CreateSupplier(CreateSupplierRequest request, int userId);
    Task<ReplyResponse> UpdateSupplier(UpdateSupplierRequest request, int userId);
}
