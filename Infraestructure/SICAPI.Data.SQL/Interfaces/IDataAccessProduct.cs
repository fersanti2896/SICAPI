using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Warehouse;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessProduct
{
    Task<ReplyResponse> CreateProduct(CreateProductRequest request, int userId);
    Task<ReplyResponse> UpdateProduct(UpdateProductRequest request, int userId);
    Task<ReplyResponse> CreateProductProvider(CreateProductProviderRequest request, int userId);
    Task<EntryResponse> CreateEntry(CreateEntryRequest request, int userId);
    Task<ReplyResponse> CreateEntryDetail(CreateEntryDetailRequest request, int userId);
}
