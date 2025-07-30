using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessSales : IDataAccessSales
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessSales(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateSale(CreateSaleRequest request, int userId)
    {
        ReplyResponse response = new();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var sale = new TSales
            {
                ClientId = request.ClientId,
                UserId = userId,
                SaleDate = NowCDMX,
                TotalAmount = request.TotalAmount,
                SaleStatusId = 2, // En proceso
                CreateDate = NowCDMX,
                CreateUser = userId,
                Status = 1,
                PaymentStatusId = 1,
                AmountPaid = 0,
                AmountPending = request.TotalAmount
            };

            Context.TSales.Add(sale);
            await Context.SaveChangesAsync();

            // Ordenar productos por ProductId para evitar deadlock
            var orderedProducts = request.Products.OrderBy(p => p.ProductId).ToList();
            var productIds = orderedProducts.Select(p => p.ProductId).ToList();

            // Cargar inventarios en memoria
            var inventories = await Context.TInventory.Where(i => productIds.Contains(i.ProductId)).ToDictionaryAsync(i => i.ProductId);

            foreach (var product in request.Products)
            {
                if (!inventories.TryGetValue(product.ProductId, out var inventory))
                    throw new Exception($"No se encontró inventario para el producto con ID {product.ProductId}");

                if ((inventory.StockReal ?? 0) < product.Quantity)
                    throw new Exception($"Stock insuficiente para el producto con ID {product.ProductId}");

                inventory.Apartado = (inventory.Apartado ?? 0) + product.Quantity;
                inventory.StockReal = (inventory.CurrentStock - inventory.Apartado) ?? 0;
                inventory.LastUpdateDate = NowCDMX;
                inventory.UpdateUser = userId;

                var saleDetail = new TSalesDetail
                {
                    SaleId = sale.SaleId,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice,
                    SubTotal = product.Quantity * product.UnitPrice,
                    CreateDate = NowCDMX,
                    CreateUser = userId,
                    Status = 1
                };

                Context.TSalesDetail.Add(saleDetail);
            }

            // ACTUALIZAR CRÉDITO DEL CLIENTE
            var client = await Context.TClients.FirstOrDefaultAsync(c => c.ClientId == request.ClientId);
            if (client == null)
                throw new Exception("Cliente no encontrado");

            if (client.AvailableCredit < sale.TotalAmount)
                throw new Exception("El cliente no tiene crédito suficiente");

            client.AvailableCredit -= sale.TotalAmount;
            client.UpdateDate = NowCDMX;
            client.UpdateUser = userId;

            // ACTUALIZAR CRÉDITO DEL VENDEDOR
            var user = await Context.TUsers.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new Exception("Vendedor no encontrado");

            if (user.AvailableCredit < sale.TotalAmount)
                throw new Exception("El vendedor no tiene crédito suficiente para registrar esta venta");

            user.AvailableCredit -= sale.TotalAmount;
            user.UpdateDate = NowCDMX;
            user.UpdateUser = userId;

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = sale.SaleId.ToString()
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al crear la venta: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "CreateSale",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesResponse> GetAllSalesByStatus(SaleByStatusRequest request, int userId)
    {
        SalesResponse response = new();

        try
        {
            var sales = await Context.TSales
                                     .Where(s => s.SaleStatusId == request.SaleStatusId)
                                     .Include(s => s.Client)
                                     .Include(s => s.SaleStatus)
                                     .Include(s => s.User)
                                     .Include(s => s.DeliveryUser) // Incluir repartidor
                                     .Select(s => new SaleDTO
                                     {
                                         SaleId = s.SaleId,
                                         ClientId = s.ClientId,
                                         BusinessName = s.Client!.BusinessName ?? "",
                                         SaleStatusId = s.SaleStatusId,
                                         StatusName = s.SaleStatus!.StatusName,
                                         TotalAmount = s.TotalAmount,
                                         SaleDate = s.SaleDate,
                                         Vendedor = s.User.FirstName + " " + (s.User.LastName ?? ""),
                                         Repartidor = s.DeliveryUser != null
                                             ? s.DeliveryUser.FirstName + " " + (s.DeliveryUser.LastName ?? "")
                                             : "Sin asignar"
                                     })
                                     .ToListAsync();


            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener ventas en proceso: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "GetAllSalesByStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<DetailsSaleResponse> DetailsSaleBySaleId(DetailsSaleRequest request, int userId)
    {
        DetailsSaleResponse response = new();

        try
        {
            var details = await Context.TSalesDetail
                                       .Include(sd => sd.Product)
                                       .Include(sd => sd.Sale!)
                                       .ThenInclude(s => s.User) // Esto trae al vendedor
                                       .Include(sd => sd.Sale!)
                                       .ThenInclude(s => s.DeliveryUser) // Trae al repartidor
                                       .Where(sd => sd.SaleId == request.SaleId)
                                       .Select(sd => new DetailsSaleDTO
                                       {
                                        SaleId = sd.SaleId,
                                        ProductId = sd.ProductId,
                                        ProductName = sd.Product!.ProductName,
                                        Quantity = sd.Quantity,
                                        UnitPrice = sd.UnitPrice,
                                        SubTotal = sd.SubTotal,
                                        Lot = sd.Lot,
                                        ExpirationDate = sd.ExpirationDate,
                                        CreateDate = sd.CreateDate,
                                        Vendedor = (sd.Sale!.User!.FirstName + " " + (sd.Sale.User.LastName ?? "")).Trim(),
                                        Repartidor = sd.Sale.DeliveryUser != null ? (sd.Sale.DeliveryUser.FirstName + " " + (sd.Sale.DeliveryUser.LastName ?? "") + " " + (sd.Sale.DeliveryUser.MLastName ?? "")).Trim() : "Sin Asignar"
                                       })
                                       .ToListAsync();

            response.Result = details;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener detalles de la venta: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "DetailsSaleBySaleId",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesStatusResponse> GetAllSalesStatus(int userId)
    {
        SalesStatusResponse response = new();

        try
        {
            var sales = await Context.TSaleStatuses
                                     .Select(s => new SalesStatusDTO { 
                                        SaleStatusId = s.SaleStatusId,
                                        StatusName = s.StatusName
                                     })
                                     .ToListAsync();

            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener listado de status de ticket: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "GetAllSalesStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> AssignDeliveryUser(AssignDeliveryUserRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == request.SaleId);

            if (sale == null)
                throw new Exception("Venta no encontrada");

            if(!request.IsUpdated)
            {
                sale.CommentsDelivery = request.CommentsDelivery;
            }

            sale.DeliveryUserId = request.DeliveryUserId;
            sale.UpdateUser = userId;
            sale.UpdateDate = NowCDMX;
            sale.SaleStatusId = 4;


            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO { Status = true, Msg = "Repartidor asignado correctamente" };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO { Code = 500, Message = $"Error: {ex.Message}" };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "AssignDeliveryUser",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> UpdateSaleStatus(UpdateSaleStatusRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var sale = await Context.TSales
                                    .Include(s => s.SaleDetails)
                                    .FirstOrDefaultAsync(s => s.SaleId == request.SaleId);

            if (sale == null)
                throw new Exception("Venta no encontrada");

            if(request.SaleStatusId == 3)
            {
                sale.Comments = request.Comments; // Primer comentario para empaquetado

                var productIds = sale.SaleDetails!.Select(d => d.ProductId).ToList();

                var inventories = await Context.TInventory
                                               .Where(i => productIds.Contains(i.ProductId))
                                               .ToDictionaryAsync(i => i.ProductId);

                foreach (var detail in sale.SaleDetails)
                {
                    if (inventories.TryGetValue(detail.ProductId, out var inventory))
                    {
                        inventory.CurrentStock -= detail.Quantity;
                        inventory.Apartado = (inventory.Apartado ?? 0) - detail.Quantity;
                        inventory.LastUpdateDate = NowCDMX;
                        inventory.StockReal = inventory.CurrentStock - (inventory.Apartado ?? 0);
                        inventory.UpdateUser = userId;
                    }
                }
            }

            sale.SaleStatusId = request.SaleStatusId;
            sale.UpdateUser = userId;
            sale.UpdateDate = NowCDMX;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO { Status = true, Msg = "Estatus actualizado correctamente" };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO { Code = 500, Message = $"Error: {ex.Message}" };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "UpdateSaleStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesPendingPaymentResponse> GetSalesPendingPayment(int userId)
    {
        var response = new SalesPendingPaymentResponse();

        try
        {
            var sales = await Context.TSales
                                     .Include(s => s.User)
                                     .Include(s => s.Client)
                                     .Include(s => s.SaleStatus)
                                     .Include(s => s.PaymentStatus)
                                     .Where(s =>
                                        new[] { 2, 3, 4, 5 }.Contains(s.SaleStatusId) &&
                                        new[] { 1, 2 }.Contains(s.PaymentStatusId) &&
                                        s.Status == 1)
                                     .Select(s => new SalesPendingPaymentDTO
                                     {
                                        SaleId = s.SaleId,
                                        SaleDate = s.SaleDate,
                                        TotalAmount = s.TotalAmount,
                                        AmountPaid = s.AmountPaid,
                                        AmountPending = s.AmountPending,
                                        SaleStatus = s.SaleStatus.StatusName,
                                        PaymentStatus = s.PaymentStatus.Name,
                                        ClientId = s.Client.ClientId,
                                        BusinessName = s.Client.BusinessName,
                                        SalesPersonId = s.User.UserId,
                                        SalesPerson = s.User.FirstName + " " + s.User.LastName
                                     })
                                     .OrderByDescending(s => s.SaleDate)
                                     .ToListAsync();


            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO { Code = 500, Message = $"Error: {ex.Message}" };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "GetSalesPendingPayment",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> ApplyPayment(ApplyPaymentRequest request, int userId)
    {
        var response = new ReplyResponse();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == request.SaleId);
            if (sale == null)
                throw new Exception("Venta no encontrada");

            if (request.Amount <= 0 || request.Amount > sale.AmountPending)
                throw new Exception("Monto de pago inválido");

            // Registrar pago
            var payment = new TPayments
            {
                SaleId = request.SaleId,
                Amount = request.Amount,
                PaymentMethod = request.Method,
                Comments = request.Comments,
                CreateDate = NowCDMX,
                CreateUser = userId,
                Status = 1
            };
            Context.TPayments.Add(payment);

            // Actualizar venta
            sale.AmountPaid += request.Amount;
            sale.AmountPending -= request.Amount;
            sale.PaymentStatusId = sale.AmountPaid == 0 ? 1 : sale.AmountPaid < sale.TotalAmount ? 2 : 3;
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            // Actualizar crédito del cliente
            var client = await Context.TClients.FirstOrDefaultAsync(c => c.ClientId == sale.ClientId);
            if (client != null)
            {
                client.AvailableCredit += request.Amount;
                client.UpdateDate = NowCDMX;
                client.UpdateUser = userId;
            }

            // Actualizar crédito del vendedor
            var seller = await Context.TUsers.FirstOrDefaultAsync(u => u.UserId == sale.UserId);
            if (seller != null)
            {
                seller.AvailableCredit += request.Amount;
                seller.UpdateDate = NowCDMX;
                seller.UpdateUser = userId;
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Pago aplicado correctamente"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al aplicar pago: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "ApplyPayment",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<MovementsSaleResponse> MovementsSaleBySaleId(DetailsSaleRequest request, int userId)
    {
        MovementsSaleResponse response = new();

        try
        {
            var sale = await Context.TSales.Where(s => s.SaleId == request.SaleId)
                                           .Select(s => new MovementsSaleDTO
                                           {
                                            SaleId = s.SaleId,
                                            Comments = s.Comments,
                                            CommentsDelivery = s.CommentsDelivery,
                                            UpdateDate = s.UpdateDate
                                           })
                                           .FirstOrDefaultAsync();

            if (sale == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "No se encontró la venta con el ID proporcionado."
                };
                return response;
            }

            response.Result = sale;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener detalles de la venta: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "MovementsSaleBySaleId",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesResponse> GetSalesByDeliveryId(SaleByStatusRequest request, int userId)
    {
        SalesResponse response = new();

        try
        {
            var limitDate = NowCDMX.Date.AddDays(-20);
            var sales = await Context.TSales
                                     .Where(s => s.SaleStatusId == request.SaleStatusId && s.DeliveryUserId == userId && s.CreateDate.Date >= limitDate)
                                     .Include(s => s.Client)
                                     .Include(s => s.SaleStatus)
                                     .Include(s => s.User)
                                     .Include(s => s.DeliveryUser) // Incluir repartidor
                                     .Select(s => new SaleDTO
                                     {
                                         SaleId = s.SaleId,
                                         ClientId = s.ClientId,
                                         BusinessName = s.Client!.BusinessName ?? "",
                                         SaleStatusId = s.SaleStatusId,
                                         StatusName = s.SaleStatus!.StatusName,
                                         TotalAmount = s.TotalAmount,
                                         SaleDate = s.SaleDate,
                                         Vendedor = s.User.FirstName + " " + (s.User.LastName ?? ""),
                                         Repartidor = s.DeliveryUser != null
                                             ? s.DeliveryUser.FirstName + " " + (s.DeliveryUser.LastName ?? "")
                                             : "Sin asignar"
                                     })
                                     .ToListAsync();


            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener ventas en transito: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "GetAllSalesByStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesByUserResponse> GetSalesByUser(SalesByUserRequest request, int userId)
    {
        SalesByUserResponse response = new();

        try
        {
            var query = Context.TSales.Where(s => s.UserId == userId && s.CreateDate.Date >= request.StartDate.Date && s.CreateDate.Date <= request.EndDate.Date);

            // Aplica el filtro por SaleStatusId solo si es diferente de 20
            if (request.SaleStatusId != 20)
                query = query.Where(s => s.SaleStatusId == request.SaleStatusId);

            var sales = await query.Include(s => s.Client)
                                   .Include(s => s.SaleStatus)
                                   .Include(s => s.PaymentStatus)
                                   .Select(s => new SalesByUserDTO {
                                    SaleId = s.SaleId,
                                    ClientId = s.ClientId,
                                    BusinessName = s.Client!.BusinessName ?? "",
                                    UserId = s.UserId,
                                    SaleDate = s.SaleDate,
                                    TotalAmount = s.TotalAmount,
                                    SaleStatusId = s.SaleStatusId,
                                    StatusName = s.SaleStatus!.StatusName,
                                    PaymentStatusId = s.PaymentStatusId,
                                    NamePayment = s.PaymentStatus!.Name
                                   })
                                   .OrderByDescending(s => s.SaleDate)
                                   .ToListAsync();

            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener ventas del usuario ({userId}): {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessSales",
                Action = "GetSalesByUser",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }
}
