using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Finance;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;
using SICAPI.Models.Response.Finance;
using SICAPI.Models.Response.Sales;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessCollection : IDataAccessCollection
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessCollection(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
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
                Module = "SICAPI-DataAccessCollection",
                Action = "ApplyPayment",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<PaymentStatusResponse> GetAllPaymentStatus(int userId)
    {
        PaymentStatusResponse response = new();

        try
        {
            var sales = await Context.TPaymentStatuses
                                     .Select(s => new PaymentStatusDTO
                                     {
                                         PaymentStatusId = s.PaymentStatusId,
                                         PaymentStatusName = s.Name
                                     })
                                     .ToListAsync();

            response.Result = sales;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener listado de status de pago: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessCollection",
                Action = "GetAllPaymentStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesPendingPaymentResponse> GetSalesPendingPayment(SalesPendingPaymentRequest request, int userId)
    {
        var response = new SalesPendingPaymentResponse();

        try
        {
            var query = Context.TSales
                           .Where(s => s.Status == 1 &&
                                       new[] { 1, 2, 4 }.Contains(s.PaymentStatusId) &&
                                       s.CreateDate.Date >= request.StartDate.Date &&
                                       s.CreateDate.Date <= request.EndDate.Date);

            if (request.ClientId.HasValue && request.ClientId.Value != 20)
                query = query.Where(s => s.ClientId == request.ClientId.Value);

            if (request.SalesPersonId.HasValue && request.SalesPersonId.Value != 20)
                query = query.Where(s => s.UserId == request.SalesPersonId.Value);

            if (request.SaleStatusId.HasValue && request.SaleStatusId.Value != 20)
                query = query.Where(s => s.SaleStatusId == request.SaleStatusId.Value);

            if (request.PaymentStatusId.HasValue && request.PaymentStatusId.Value != 20)
                query = query.Where(s => s.PaymentStatusId == request.PaymentStatusId.Value);

            var sales = await query.Include(s => s.User)
                                   .Include(s => s.Client)
                                   .Include(s => s.SaleStatus)
                                   .Include(s => s.PaymentStatus)
                                   .Select(s => new SalesPendingPaymentDTO
                                   {
                                        SaleId = s.SaleId,
                                        SaleDate = s.SaleDate,
                                        TotalAmount = s.TotalAmount,
                                        AmountPaid = s.AmountPaid,
                                        AmountPending = s.AmountPending,
                                        SaleStatus = s.SaleStatus.StatusName,
                                        PaymentStatusId = s.PaymentStatusId,
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
                Module = "SICAPI-DataAccessCollection",
                Action = "GetSalesPendingPayment",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesPendingPaymentResponse> GetSalesHistorical(SalesHistoricalRequest request, int userId)
    {
        var response = new SalesPendingPaymentResponse();

        try
        {
            var query = Context.TSales
                           .Where(s => s.Status == 1 && s.CreateDate.Date >= request.StartDate.Date && s.CreateDate.Date <= request.EndDate.Date);

            if (request.ClientId.HasValue && request.ClientId.Value != 20)
                query = query.Where(s => s.ClientId == request.ClientId.Value);

            if (request.SalesPersonId.HasValue && request.SalesPersonId.Value != 20)
                query = query.Where(s => s.UserId == request.SalesPersonId.Value);

            if (request.SaleStatusId.HasValue && request.SaleStatusId.Value != 20)
                query = query.Where(s => s.SaleStatusId == request.SaleStatusId.Value);

            if (request.PaymentStatusId.HasValue && request.PaymentStatusId.Value != 20)
                query = query.Where(s => s.PaymentStatusId == request.PaymentStatusId.Value);

            var sales = await query.Include(s => s.User)
                                   .Include(s => s.Client)
                                   .Include(s => s.SaleStatus)
                                   .Include(s => s.PaymentStatus)
                                   .Select(s => new SalesPendingPaymentDTO
                                   {
                                       SaleId = s.SaleId,
                                       SaleDate = s.SaleDate,
                                       TotalAmount = s.TotalAmount,
                                       AmountPaid = s.AmountPaid,
                                       AmountPending = s.AmountPending,
                                       SaleStatus = s.SaleStatus.StatusName,
                                       PaymentStatusId = s.PaymentStatusId,
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
                Module = "SICAPI-DataAccessCollection",
                Action = "GetSalesHistorical",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<SalesPendingPaymentResponse> GetSalesPaids(SalesHistoricalRequest request, int userId)
    {
        var response = new SalesPendingPaymentResponse();

        try
        {
            var query = Context.TSales
                           .Where(s => s.Status == 1 && s.PaymentStatusId == 3 && s.CreateDate.Date >= request.StartDate.Date && s.CreateDate.Date <= request.EndDate.Date);

            if (request.ClientId.HasValue && request.ClientId.Value != 20)
                query = query.Where(s => s.ClientId == request.ClientId.Value);

            if (request.SalesPersonId.HasValue && request.SalesPersonId.Value != 20)
                query = query.Where(s => s.UserId == request.SalesPersonId.Value);


            var sales = await query.Include(s => s.User)
                                   .Include(s => s.Client)
                                   .Include(s => s.SaleStatus)
                                   .Include(s => s.PaymentStatus)
                                   .Select(s => new SalesPendingPaymentDTO
                                   {
                                       SaleId = s.SaleId,
                                       SaleDate = s.SaleDate,
                                       TotalAmount = s.TotalAmount,
                                       AmountPaid = s.AmountPaid,
                                       AmountPending = s.AmountPending,
                                       SaleStatus = s.SaleStatus.StatusName,
                                       PaymentStatusId = s.PaymentStatusId,
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
                Module = "SICAPI-DataAccessCollection",
                Action = "GetSalesPaids",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> CancelSaleWithComment(CancelSaleRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == request.SaleId);
            if (sale == null)
                throw new Exception("Venta no encontrada");

            // Cambiar estatus a 'Cancelado pendiente de devolución' (estatus 6)
            sale.SaleStatusId = 6;
            sale.PaymentStatusId = 5;
            sale.UpdateUser = userId;
            sale.UpdateDate = NowCDMX;

            // Guardar comentario
            var comment = new TCancelledSalesComments
            {
                SaleId = sale.SaleId,
                Comments = request.Comments,
                Status = 1,
                CreateUser = userId,
                CreateDate = NowCDMX
            };

            Context.TCancelledSalesComments.Add(comment);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Venta cancelada correctamente"
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO { Code = 500, Message = $"Error: {ex.Message}" };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessCollection",
                Action = "CancelSaleWithComment",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<CancelledSaleCommentResponse> GetCancelledSaleComments(CancelledCommentsRequest request, int userId)
    {
        var response = new CancelledSaleCommentResponse();

        try
        {
            var comments = await Context.TCancelledSalesComments
                                        .Where(c => c.SaleId == request.SaleId && c.Status == 1)
                                        .OrderByDescending(c => c.CreateDate)
                                        .Select(c => new CancelledSaleCommentDTO
                                        {
                                            Comments = c.Comments,
                                            CreateDate = c.CreateDate
                                        })
                                        .ToListAsync();

            response.Result = comments;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al obtener comentarios de cancelación: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessCollection",
                Action = "GetCancelledSaleComments",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<ReplyResponse> CancelSaleByOmission(CancelSaleRequest request, int userId)
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

            if (sale.SaleStatusId != 2 && sale.SaleStatusId != 3)
            {
                response.Error = new ErrorDTO { Code = 400, Message = "Solo se puede cancelar por omisión en estatus En Proceso (2) o Empaquetado (3)" };
                return response;
            }

            var productIds = sale.SaleDetails!.Select(d => d.ProductId).ToList();

            var inventories = await Context.TInventory
                                           .Where(i => productIds.Contains(i.ProductId))
                                           .ToDictionaryAsync(i => i.ProductId);

            foreach (var detail in sale.SaleDetails)
            {
                if (inventories.TryGetValue(detail.ProductId, out var inventory))
                {
                    if (sale.SaleStatusId == 2)
                        inventory.Apartado = (inventory.Apartado ?? 0) - detail.Quantity;
                    else if (sale.SaleStatusId == 3)
                    {
                        inventory.CurrentStock += detail.Quantity;
                        inventory.Apartado = (inventory.Apartado ?? 0) - detail.Quantity;
                    }

                    inventory.StockReal = inventory.CurrentStock - (inventory.Apartado ?? 0);
                    inventory.LastUpdateDate = NowCDMX;
                    inventory.UpdateUser = userId;
                }
            }

            // Revertir crédito al cliente
            if (sale.Client != null)
            {
                sale.Client.AvailableCredit += sale.TotalAmount;
                sale.Client.UpdateDate = NowCDMX;
                sale.Client.UpdateUser = userId;
            }

            // Revertir crédito al vendedor
            if (sale.User != null)
            {
                sale.User.AvailableCredit += sale.TotalAmount;
                sale.User.UpdateDate = NowCDMX;
                sale.User.UpdateUser = userId;
            }

            // Guardar comentario
            Context.TCancelledSalesComments.Add(new TCancelledSalesComments
            {
                SaleId = request.SaleId,
                Comments = request.Comments,
                CreateDate = NowCDMX,
                CreateUser = userId,
                Status = 1
            });

            // Cambiar estatus del ticket a 10 = Cancelado por omisión
            sale.SaleStatusId = 10;
            sale.PaymentStatusId = 5;
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Cancelación por omisión completada. Stock y créditos revertidos correctamente."
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al cancelar por omisión: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessCollection",
                Action = "CancelSaleByOmission",
                Message = $"Exception: {ex.Message}",
                InnerException = ex.InnerException?.Message
            });
        }

        return response;
    }

    public async Task<FinanceBuildResponse> GetFinanceSummaryAsync(FinanceBuildRequest request, int userId)
    {
        var response = new FinanceBuildResponse();

        try
        {
            var summary = await Context.TPayments
                                       .Where(p => p.Status == 1 &&
                                                    p.PaymentDate.Date >= request.StartDate.Date &&
                                                    p.PaymentDate.Date <= request.EndDate.Date)
                                       .GroupBy(p => p.PaymentMethod)
                                       .Select(g => new FinanceMethodTotalDTO
                                       {
                                        PaymentMethod = g.Key,
                                        TotalAmount = g.Sum(p => p.Amount)
                                       })
                                       .ToListAsync();

            response.Result = summary;
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al generar el resumen financiero: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessFinance",
                Action = "GetFinanceSummaryAsync",
                Message = $"Excepción: {ex.Message}",
                InnerException = ex.InnerException?.Message,
                IdUser = userId
            });
        }

        return response;
    }

}
