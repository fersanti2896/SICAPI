using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Request.Warehouse;
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "UpdateSaleStatus",
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
                IdUser = userId,
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
                IdUser = userId,
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

            // Filtro por SaleStatusId si es válido y diferente de 20
            if (request.SaleStatusId.HasValue && request.SaleStatusId.Value != 20)
                query = query.Where(s => s.SaleStatusId == request.SaleStatusId.Value);

            // Filtro por PaymentStatusId si es válido y diferente de 20
            if (request.PaymentStatusId.HasValue && request.PaymentStatusId.Value != 20)
                query = query.Where(s => s.PaymentStatusId == request.PaymentStatusId.Value);

            var sales = await query.Include(s => s.Client)
                                   .Include(s => s.SaleStatus)
                                   .Include(s => s.PaymentStatus)
                                   .Select(s => new SalesByUserDTO
                                   {
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
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "GetSalesByUser",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> ConfirmReturnAndRevertStock(CancelSaleRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var sale = await Context.TSales
                                    .Include(s => s.SaleDetails)
                                    .Include(s => s.Client)
                                    .Include(s => s.User)
                                    .FirstOrDefaultAsync(s => s.SaleId == request.SaleId);

            if (sale == null)
            {
                response.Error = new ErrorDTO { Code = 404, Message = "Venta no encontrada" };

                return response;
            }

            if (sale.SaleStatusId != 6) // Estatus 6 = Cancelado pendiente de devolución
            {
                response.Error = new ErrorDTO { Code = 400, Message = "Solo se pueden confirmar devoluciones pendientes" };

                return response;
            }

            foreach (var detail in sale.SaleDetails!)
            {
                var inventory = await Context.TInventory.FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);
                if (inventory != null)
                {
                    inventory.CurrentStock += detail.Quantity;
                    inventory.StockReal = inventory.CurrentStock - (inventory.Apartado ?? 0);
                    inventory.LastUpdateDate = NowCDMX;
                    inventory.UpdateUser = userId;
                }
            }

            if (sale.Client != null)
                sale.Client.AvailableCredit += sale.TotalAmount;

            if (sale.User != null)
                sale.User.AvailableCredit += sale.TotalAmount;

            sale.SaleStatusId = 9; // Estatus 9 = Cancelado con devolución confirmada
            sale.PaymentStatusId = 5;
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            Context.TCancelledSalesComments.Add(new TCancelledSalesComments
            {
                SaleId = request.SaleId,
                Comments = request.Comments,
                Status = 1,
                CreateDate = NowCDMX,
                CreateUser = userId
            });

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Devolución confirmada y stock actualizado correctamente"
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al confirmar devolución: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "ConfirmReturnAndRevertStockAsync",
                Message = $"Excepción: {ex.Message}",
                InnerException = ex.InnerException?.Message
            });
        }

        return response;
    }

    public async Task<ReplyResponse> CreateCreditNoteRequest(CreditNoteRequest request, int userId)
    {
        ReplyResponse response = new();

        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == request.SaleId);

            if (sale == null)
            {
                response.Error = new ErrorDTO { Code = 404, Message = "Venta no encontrada" };

                return response;
            }

            sale.SaleStatusId = 11; // Estatus 11 = Nota de crédito pendiente
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            decimal total = request.Products.Sum(p => p.Quantity * p.UnitPrice);

            var note = new TNotesCreditRequests
            {
                SaleId = request.SaleId,
                FinalCreditAmount = total,
                Comments = request.Comments,
                CreateUser = userId,
                CreateDate = NowCDMX,
                Status = 11,
                IsApproved = false
            };

            Context.TNotesCreditRequests.Add(note);
            await Context.SaveChangesAsync();

            foreach (var product in request.Products)
            {
                var detail = new TNotesCreditDetails
                {
                    NoteCreditId = note.NoteCreditId,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice
                };
                Context.TNotesCreditDetails.Add(detail);
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Nota de crédito solicitada correctamente."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al crear la nota de crédito: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "CreateCreditNoteRequest",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> ConfirmCreditNoteByWarehouse(ConfirmCreditNoteRequest request, int userId)
    {
        var response = new ReplyResponse();

        await using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var note = await Context.TNotesCreditRequests
                                    .Include(n => n.Details)
                                    .Include(n => n.Sale)
                                        .ThenInclude(s => s.Client)
                                    .Include(n => n.Sale)
                                        .ThenInclude(s => s.User)
                                    .FirstOrDefaultAsync(n => n.NoteCreditId == request.NoteCreditId);

            if (note == null || note.Status != 12)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "La nota de crédito no existe o no ha sido aprobada por cobranza."
                };
                return response;
            }

            foreach (var item in note.Details)
            {
                var inventory = await Context.TInventory.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventory == null)
                {
                    response.Error = new ErrorDTO
                    {
                        Code = 404,
                        Message = $"No se encontró inventario para el producto con ID {item.ProductId}"
                    };
                    return response;
                }

                inventory.CurrentStock += item.Quantity;
                inventory.StockReal = (inventory.StockReal ?? 0) + item.Quantity;
                inventory.LastUpdateDate = NowCDMX;
            }

            // Actualizar la nota de crédito
            note.Status = 13;
            note.ApprovedByUserId = userId;
            note.ApprovedDate = NowCDMX;
            note.CommentsDevolution = request.CommentsDevolution;
            note.IsApproved = true;

            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == note.SaleId);

            if (sale == null)
            {
                response.Error = new ErrorDTO { Code = 404, Message = "Venta no encontrada" };

                return response;
            }

            sale.SaleStatusId = 13;
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            note.Sale.AmountPending -= note.FinalCreditAmount;
            if (note.Sale.AmountPending < 0)
                note.Sale.AmountPending = 0;

            // Actualizar crédito del cliente
            var client = note.Sale!.Client!;
            client.AvailableCredit += note.FinalCreditAmount;

            // Actualizar crédito del vendedor
            var user = note.Sale!.User!;
            user.AvailableCredit += note.FinalCreditAmount;

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "La nota de crédito fue confirmada por almacén y el stock fue actualizado correctamente."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "ConfirmCreditNoteByWarehouse",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error al confirmar la nota de crédito desde almacén."
            };
        }

        return response;
    }

    public async Task<DetailsNoteCreditResponse> DetailsNoteCreditById(DetailsNoteCreditRequest request, int userId)
    {
        DetailsNoteCreditResponse response = new();

        try
        {
            var details = await (from n in Context.TNotesCreditRequests
                                 join nd in Context.TNotesCreditDetails on n.NoteCreditId equals nd.NoteCreditId
                                 join p in Context.TProducts on nd.ProductId equals p.ProductId
                                 join u in Context.TUsers on n.CreateUser equals u.UserId
                                 where n.NoteCreditId == request.NoteCreditId
                                 select new DetailsNoteCreditDTO
                                 {
                                     NoteCreditId = n.NoteCreditId,
                                     SaleId = n.SaleId,
                                     ProductId = nd.ProductId,
                                     ProductName = p.ProductName,
                                     Quantity = nd.Quantity,
                                     UnitPrice = nd.UnitPrice,
                                     SubTotal = nd.SubTotal,
                                     CreateDate = n.CreateDate,
                                     CreateUser = n.CreateUser,
                                     CreadoPor = u.FirstName + " " + (u.LastName ?? ""),
                                     FinalCreditAmount = n.FinalCreditAmount
                                 }).ToListAsync();

            response.Result = details;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener detalles de la nota de crédito: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessSales",
                Action = "DetailsNoteCreditById",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }
}
