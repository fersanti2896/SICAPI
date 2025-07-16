using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Products;
using SICAPI.Models.Response.Warehouse;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessProduct : IDataAccessProduct
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }

    public DataAccessProduct(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateProduct(CreateProductRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var product = new TProducts
            {
                ProductName = request.ProductName,
                Description = request.Description,
                Barcode = request.Barcode ?? null,
                Category = request.Unit,
                Price = request.Price,
                CreateDate = DateTime.Now,
                CreateUser = userId,
                Status = 1
            };

            await Context.TProducts.AddAsync(product);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Producto creado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "CreateProduct",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO { Code = 500, Message = ex.Message };
        }

        return response;
    }

    public async Task<ReplyResponse> UpdateProduct(UpdateProductRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var product = await Context.TProducts.FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.Status == 1);

            if (product == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Producto no encontrado o inactivo."
                };
                return response;
            }

            product.ProductName = request.ProductName;
            product.Description = request.Description;
            product.Barcode = request.Barcode;
            product.Presentation = request.Presentation;
            product.Category = request.Unit;
            product.Price = request.Price;
            product.UpdateDate = DateTime.Now;
            product.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Producto actualizado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "UpdateProduct",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al actualizar producto: {ex.Message}"
            };
        }

        return response;
    }


    public async Task<ReplyResponse> CreateProductProvider(CreateProductProviderRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            // Validar si ya existe combinación producto-proveedor con la misma clave
            var exists = await Context.TProductProviders.AnyAsync(p => p.ProductId == request.ProductId && p.SupplierId == request.SupplierId);

            if (exists)
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "Ya existe un registro con esa clave de proveedor para este producto."
                };
                return response;
            }

            var entity = new TProductProviders
            {
                ProductId = request.ProductId,
                SupplierId = request.SupplierId,
                ProviderKey = request.ProviderKey,
                ProviderDescription = request.ProviderDescription,
                UnitPrice = request.UnitPrice,
                Unit = request.Unit,
                Status = 1,
                CreateDate = DateTime.Now,
                CreateUser = userId
            };

            Context.TProductProviders.Add(entity);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Proveedor del producto registrado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "CreateProductProvider",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar proveedor del producto: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<ReplyResponse> CreateFullEntry(CreateEntryRequest request, int userId)
    {
        ReplyResponse response = new();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var entry = new TEntradasAlmacen
            {
                SupplierId = request.SupplierId,
                InvoiceNumber = request.InvoiceNumber,
                EntryDate = DateTime.Now,
                ExpectedPaymentDate = request.ExpectedPaymentDate,
                TotalAmount = request.TotalAmount,
                Observations = request.Observations,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            await Context.TEntradasAlmacen.AddAsync(entry);
            await Context.SaveChangesAsync();

            foreach (var prod in request.Products)
            {
                var subtotal = prod.Quantity * prod.UnitPrice;
                var entryDetail = new TEntradaDetalle
                {
                    EntryId = entry.EntryId,
                    ProductId = prod.ProductId,
                    Quantity = prod.Quantity,
                    UnitPrice = prod.UnitPrice,
                    SubTotal = subtotal,
                    Lot = prod.Lot,
                    ExpirationDate = prod.ExpirationDate,
                    Status = 1,
                    CreateDate = DateTime.Now,
                    CreateUser = userId
                };

                Context.TEntradaDetalle.Add(entryDetail);

                var inventory = await Context.TInventory.FirstOrDefaultAsync(x => x.ProductId == prod.ProductId);

                if (inventory != null)
                {
                    inventory.CurrentStock += prod.Quantity;
                    inventory.LastEntryDate = DateTime.Now;
                    inventory.LastUpdateDate = DateTime.Now;
                    inventory.UpdateUser = userId;
                    inventory.StockReal = inventory.CurrentStock - inventory.Apartado;
                }
                else
                {
                    inventory = new TInventory
                    {
                        ProductId = prod.ProductId,
                        CurrentStock = prod.Quantity,
                        Apartado = 0,
                        StockReal = prod.Quantity,
                        LastEntryDate = DateTime.Now,
                        LastUpdateDate = DateTime.Now,
                        CreateDate = DateTime.Now,
                        CreateUser = userId,
                        Status = 1
                    };
                    Context.TInventory.Add(inventory);
                }
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Nota de pedido registrada con sus productos",
                Status = true
            };

            return response;            
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "CreateEntry",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar la nota de pedido: {ex.Message}"
            };
        }
        
        return response;
    }

    public async Task<ProductsResponse> GetAllProducts(int userId)
    {
        ProductsResponse response = new();

        try
        {
            var products = await Context.TProducts
                                        .Select(u => new ProductDTO
                                        {
                                            ProductId = u.ProductId,
                                            ProductName = u.ProductName,
                                            Barcode = u.Barcode,
                                            Unit = u.Category,
                                            Price = u.Price,
                                            Description = u.Description
                                        })
                                        .ToListAsync();

            response.Result = products;

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "GetAllProducts",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            return new ProductsResponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }

    }

    public async Task<StockResponse> GetStock(int userId)
    {
        StockResponse response = new();

        try
        {
            var stock = await Context.TInventory
                                     .Include(u => u.Product)
                                     .Select(u => new StockDTO
                                     {
                                         InventoryId = u.InventoryId,
                                         ProductName = u.Product.ProductName,
                                         Description = u.Product.Description ?? string.Empty,
                                         CurrentStock = u.CurrentStock,
                                         Apartado = u.Apartado,
                                         StockReal = u.StockReal,
                                         LastUpdateDate = u.LastUpdateDate
                                     })
                                    .ToListAsync();

            response.Result = stock;

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "GetStock",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            return new StockResponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }
    }

    public async Task<StockRealResponse> GetStockReal(int userId)
    {
        StockRealResponse response = new();

        try
        {
            var stock = await Context.TInventory
                                     .Include(u => u.Product)
                                     .Where(u => u.StockReal > 0)
                                     .Select(u => new StockRealDTO
                                     {
                                         ProductId = u.ProductId,
                                         ProductName = u.Product.ProductName,
                                         Price = u.Product.Price ?? 0,
                                         StockReal = u.StockReal ?? 0,
                                     })
                                    .ToListAsync();

            response.Result = stock;

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "GetStockReal",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            return new StockRealResponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }
    }
    
    public async Task<DetailsEntryResponse> FullEntryById(DetailsEntryRequest request, int userId) {
        DetailsEntryResponse response = new();

        try
        {
            var entry = await Context.TEntradasAlmacen.Where(e => e.EntryId == request.EntryId).Include(e => e.Supplier).FirstOrDefaultAsync();

            if (entry == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Nota de pedido no encontrada."
                };
                return response;
            }

            var details = await Context.TEntradaDetalle.Where(d => d.EntryId == request.EntryId).Include(d => d.Product).ToListAsync();

            var detailsList = details.Select(d => new ProductsDetailsEntryDTO
            {
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "Sin nombre",
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                SubTotal = d.SubTotal,
                Lot = d.Lot,
                ExpirationDate = d.ExpirationDate
            }).ToList();

            response.Result = new DetailsEntryDTO
            {
                EntryId = entry.EntryId,
                SupplierId = entry.SupplierId,
                BusinessName = entry.Supplier?.BusinessName ?? "Proveedor no identificado",
                InvoiceNumber = entry.InvoiceNumber ?? "",
                EntryDate = entry.EntryDate,
                ExpectedPaymentDate = entry.ExpectedPaymentDate ?? DateTime.MinValue,
                TotalAmount = entry.TotalAmount,
                Observations = entry.Observations ?? "",
                ProductsDetails = detailsList
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "FullEntryById",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener los detalles de la nota de pedido: {ex.Message}"
            };
        }

        return response;
    }
}
