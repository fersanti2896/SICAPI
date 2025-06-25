using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Warehouse;
using SICAPI.Models.Response;
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
                Barcode = request.Barcode,
                Presentation = request.Presentation,
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

    public async Task<EntryResponse> CreateEntry(CreateEntryRequest request, int userId)
    {
        EntryResponse response = new();

        try
        {
            var entity = new TEntradasAlmacen
            {
                SupplierId = request.SupplierId,
                InvoiceNumber = request.InvoiceNumber,
                EntryDate = request.EntryDate,
                ExpectedPaymentDate = request.ExpectedPaymentDate,
                TotalAmount = request.TotalAmount,
                Observations = request.Observations,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            await Context.TEntradasAlmacen.AddAsync(entity);
            await Context.SaveChangesAsync();


            response.Result = new EntryDTO
            {
                EntryId = entity.EntryId,
                InvoiceNumber = entity.InvoiceNumber,
                TotalAmount = entity.TotalAmount,
                EntryDate = entity.EntryDate
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

    public async Task<ReplyResponse> CreateEntryDetail(CreateEntryDetailRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var subtotal = request.Quantity * request.UnitPrice;

            var entity = new TEntradaDetalle
            {
                EntryId = request.EntryId,
                ProductProviderId = request.ProductProviderId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                SubTotal = subtotal,
                CreateDate = DateTime.Now,
                Status = 1,
                CreateUser = userId
            };

            Context.TEntradaDetalle.Add(entity);

            // Actualizar o insertar en inventario
            var productId = await Context.TProductProviders
                                .Where(p => p.ProductProviderId == request.ProductProviderId)
                                .Select(p => p.ProductId)
                                .FirstOrDefaultAsync();

            if (productId == 0)
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "No se encontró el producto asociado al proveedor."
                };

                return response;
            }

            var inventory = await Context.TInventory.FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory != null)
            {
                inventory.CurrentStock += request.Quantity;
                inventory.LastEntryDate = DateTime.Now;
                inventory.LastUpdateDate = DateTime.Now;
                inventory.UpdateUser = userId;
            }
            else
            {
                inventory = new TInventory
                {
                    ProductId = productId,
                    CurrentStock = request.Quantity,
                    LastEntryDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    Status = 1,
                    CreateDate = DateTime.Now,
                    CreateUser = userId
                };
                Context.TInventory.Add(inventory);
            }

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = "Detalle de entrada registrado correctamente.",
                Status = true
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessProduct",
                Action = "CreateEntryDetail",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al registrar detalle de entrada: {ex.Message}"
            };
        }

        return response;
    }
}
