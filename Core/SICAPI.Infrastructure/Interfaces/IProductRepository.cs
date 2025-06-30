
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Products;
using SICAPI.Models.Response.Supplier;
using SICAPI.Models.Response.Warehouse;

namespace SICAPI.Infrastructure.Interfaces;

public interface IProductRepository
{
    Task<ReplyResponse> CreateProduct(CreateProductRequest request, int userId);
    Task<ReplyResponse> CreateProductProvider(CreateProductProviderRequest request, int userId);
    Task<ReplyResponse> CreateFullEntry(CreateEntryRequest request, int userId);
    Task<ReplyResponse> UpdateProduct(UpdateProductRequest request, int userId);
    Task<ProductsResponse> GetAllProducts(int userId);
    Task<ProductsProvidersResponse> GetProductsBySupplierId(ProductsBySupplierRequest request, int userId);
}

