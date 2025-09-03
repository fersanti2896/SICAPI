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
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

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
                CreateDate = NowCDMX,
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
                IdUser = userId,
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
            product.UpdateDate = NowCDMX;
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
                IdUser = userId,
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
                CreateDate = NowCDMX,
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
                IdUser = userId,
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
                EntryDate = NowCDMX,
                ExpectedPaymentDate = request.ExpectedPaymentDate,
                TotalAmount = request.TotalAmount,
                Observations = request.Observations,
                CreateDate = NowCDMX,
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
                    CreateDate = NowCDMX,
                    CreateUser = userId
                };

                Context.TEntradaDetalle.Add(entryDetail);

                var inventory = await Context.TInventory.FirstOrDefaultAsync(x => x.ProductId == prod.ProductId);

                if (inventory != null)
                {
                    inventory.CurrentStock += prod.Quantity;
                    inventory.LastEntryDate = NowCDMX;
                    inventory.LastUpdateDate = NowCDMX;
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
                        LastEntryDate = NowCDMX,
                        LastUpdateDate = NowCDMX,
                        CreateDate = NowCDMX,
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
                IdUser = userId,
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
                                            Description = u.Description,
                                            Status = u.Status
                                        })
                                        .ToListAsync();

            response.Result = products;

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
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
                IdUser = userId,
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
                                     .Where(u => u.StockReal > 0 && u.Product.Status != 0)
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
                IdUser = userId,
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
                EntryDetailId = d.EntryDetailId,
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
                IdUser = userId,
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

    public async Task<ReplyResponse> UpdateEntryPrices(UpdateEntryPricesRequest request, int userId)
    {
        var response = new ReplyResponse();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var entry = await Context.TEntradasAlmacen
                                     .FirstOrDefaultAsync(e => e.EntryId == request.EntryId);

            if (entry == null)
                throw new Exception("Nota de entrada no encontrada");

            decimal newTotal = 0;

            foreach (var item in request.Products)
            {
                var detail = Context.TEntradaDetalle.FirstOrDefault(d => d.EntryDetailId == item.EntryDetailId);
                if (detail == null)
                    throw new Exception($"Detalle con ID {item.EntryDetailId} no encontrado");

                detail.UnitPrice = item.UnitPrice;
                detail.SubTotal = item.UnitPrice * detail.Quantity;
                detail.UpdateDate = NowCDMX;
                detail.UpdateUser = userId;

                newTotal += detail.SubTotal;
            }

            entry.TotalAmount = newTotal;
            entry.AmountPaid = 0;
            entry.AmountPending = newTotal;
            entry.Observations = request.Observations;
            entry.ExpectedPaymentDate = request.ExpectedPaymentDate;
            entry.UpdateDate = NowCDMX;
            entry.UpdateUser = userId;

            var supplier = await Context.TSuppliers.FirstOrDefaultAsync(s => s.SupplierId == entry.SupplierId);
            if (supplier == null)
                throw new Exception($"Proveedor {entry.SupplierId} no encontrado");

            supplier.Balance -= newTotal;
            supplier.UpdateDate = NowCDMX;
            supplier.UpdateUser = userId;

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Precios actualizados correctamente"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al actualizar precios: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessProduct",
                Action = "UpdateEntryPrices",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> DeactivateProduct(ActiveProductRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var product = await Context.TProducts.FirstOrDefaultAsync(u => u.ProductId == request.ProductId);

            if (product == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Producto no encontrado"
                };
                return response;
            }

            product.Status = request.Status;
            product.UpdateDate = NowCDMX;
            product.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = request.Status == 1 ? "Producto activado correctamente" : "Producto desactivado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al desactivar producto: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessProduct",
                Action = "DeactivateProduct",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }
}
