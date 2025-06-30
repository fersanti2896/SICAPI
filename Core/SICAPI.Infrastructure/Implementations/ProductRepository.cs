using SICAPI.Data.SQL.Implementations;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Products;
using SICAPI.Models.Response.Supplier;
using SICAPI.Models.Response.Warehouse;

namespace SICAPI.Infrastructure.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly IDataAccessProduct IDataAccessProduct;
    private IDataAccessLogs IDataAccessLogs;

    public ProductRepository(IDataAccessProduct iDataAccessProduct, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessProduct = iDataAccessProduct;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<ProductsResponse> GetAllProducts(int userId)
    {
        ProductsResponse response = new();
        try
        {
            response = await IDataAccessProduct.GetAllProducts(userId);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-ProductRepository",
                Action = "GetAllProducts",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };

            return response;
        }
    }

    public Task<ReplyResponse> CreateProduct(CreateProductRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessProduct.CreateProduct(request, userId), "CreateProduct", userId);
    }
    public Task<ReplyResponse> UpdateProduct(UpdateProductRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessProduct.UpdateProduct(request, userId), "UpdateProduct", userId);
    }

    public Task<ReplyResponse> CreateProductProvider(CreateProductProviderRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessProduct.CreateProductProvider(request, userId), "CreateProductProvider", userId);
    }

    public Task<ReplyResponse> CreateFullEntry(CreateEntryRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessProduct.CreateFullEntry(request, userId), "CreateFullEntry", userId);
    }

    public Task<ProductsProvidersResponse> GetProductsBySupplierId(ProductsBySupplierRequest request, int userId)
    {
        return ExecuteWithLogging(() => IDataAccessProduct.GetProductsBySupplierId(request, userId), "GetProductsBySupplierId", userId);
    }

    private async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string actionName, int userId) where T : BaseResponse, new()
    {
        T response = new();

        try
        {
            response = await action();
            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-ProductRepository",
                Action = actionName,
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
            return response;
        }
    }
}
