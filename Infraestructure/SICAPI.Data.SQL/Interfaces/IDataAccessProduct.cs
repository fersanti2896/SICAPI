﻿using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Products;
using SICAPI.Models.Response.Warehouse;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessProduct
{
    Task<ReplyResponse> CreateProduct(CreateProductRequest request, int userId);
    Task<ReplyResponse> UpdateProduct(UpdateProductRequest request, int userId);
    Task<ReplyResponse> CreateProductProvider(CreateProductProviderRequest request, int userId);
    Task<ReplyResponse> CreateFullEntry(CreateEntryRequest request, int userId);
    Task<ProductsResponse> GetAllProducts(int userId);
    Task<StockResponse> GetStock(int userId);
    Task<StockRealResponse> GetStockReal(int userId);
    Task<DetailsEntryResponse> FullEntryById(DetailsEntryRequest request, int userId);
}
