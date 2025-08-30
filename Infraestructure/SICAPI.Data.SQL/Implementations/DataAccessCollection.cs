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
                PaymentDate = request.PaymentDate ?? NowCDMX,
                CreateUser = userId,
                Status = 1
            };

            // Si el método es pago cuenta de tercero, se asigna el proveedor
            if (request.Method == "Pago Cuenta de Tercero")
            {
                if (request.ThirdPartySupplierId == null)
                    throw new Exception("Debe proporcionar el proveedor para Pago Cuenta de Tercero");

                payment.ThirdPartySupplierId = request.ThirdPartySupplierId;

                // Actualizar saldo del proveedor (como una “cuenta de banco”)
                var supplier = await Context.TSuppliers.FirstOrDefaultAsync(s => s.SupplierId == request.ThirdPartySupplierId);

                if (supplier == null)
                    throw new Exception($"Proveedor no encontrado con ID {request.ThirdPartySupplierId}");

                supplier.ThirdPartyBalance += request.Amount;
                supplier.UpdateDate = NowCDMX;
                supplier.UpdateUser = userId;
            }


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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
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
                IdUser = userId,
                Module = "SICAPI-DataAccessCollection",
                Action = "CancelSaleByOmission",
                Message = $"Exception: {ex.Message}",
                InnerException = ex.InnerException?.Message
            });
        }

        return response;
    }

    public async Task<FinanceBuildResponse> GetFinanceSummary(FinanceBuildRequest request, int userId)
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
                Action = "GetFinanceSummary",
                Message = $"Excepción: {ex.Message}",
                InnerException = ex.InnerException?.Message,
                IdUser = userId
            });
        }

        return response;
    }

    public async Task<ReplyResponse> ApplyMultiplePayments(ApplyMultiplePaymentRequest request, int userId)
    {
        var response = new ReplyResponse();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            foreach (var saleDto in request.Sales)
            {
                var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == saleDto.SaleId);
                if (sale == null)
                    throw new Exception($"Venta no encontrada con ID {saleDto.SaleId}");

                if (saleDto.Amount <= 0 || saleDto.Amount > sale.AmountPending)
                    throw new Exception($"Monto inválido para la venta ID {saleDto.SaleId}");

                // Registrar pago
                var payment = new TPayments
                {
                    SaleId = sale.SaleId,
                    Amount = saleDto.Amount,
                    PaymentMethod = request.Method,
                    Comments = request.Comments,
                    PaymentDate = request.PaymentDate,
                    CreateDate = NowCDMX,
                    CreateUser = userId,
                    Status = 1
                };

                // Si el método es pago cuenta de tercero, se asigna el proveedor
                if (request.Method == "Pago Cuenta de Tercero")
                {
                    if (request.ThirdPartySupplierId == null)
                        throw new Exception("Debe proporcionar el proveedor para Pago Cuenta de Tercero");

                    payment.ThirdPartySupplierId = request.ThirdPartySupplierId;

                    // Actualizar saldo del proveedor (como una “cuenta de banco”)
                    var supplier = await Context.TSuppliers.FirstOrDefaultAsync(s => s.SupplierId == request.ThirdPartySupplierId);

                    if (supplier == null)
                        throw new Exception($"Proveedor no encontrado con ID {request.ThirdPartySupplierId}");

                    supplier.ThirdPartyBalance += saleDto.Amount;
                    supplier.UpdateDate = NowCDMX;
                    supplier.UpdateUser = userId;
                }

                Context.TPayments.Add(payment);

                // Actualizar venta
                sale.AmountPaid += saleDto.Amount;
                sale.AmountPending -= saleDto.Amount;
                sale.PaymentStatusId = sale.AmountPaid == 0 ? 1 : sale.AmountPaid < sale.TotalAmount ? 2 : 3;
                sale.UpdateDate = NowCDMX;
                sale.UpdateUser = userId;

                // Actualizar crédito del cliente
                var client = await Context.TClients.FirstOrDefaultAsync(c => c.ClientId == sale.ClientId);

                if (client != null)
                {
                    client.AvailableCredit += saleDto.Amount;
                    client.UpdateDate = NowCDMX;
                    client.UpdateUser = userId;
                }

                // Actualizar crédito del vendedor
                var seller = await Context.TUsers.FirstOrDefaultAsync(u => u.UserId == sale.UserId);

                if (seller != null)
                {
                    seller.AvailableCredit += saleDto.Amount;
                    seller.UpdateDate = NowCDMX;
                    seller.UpdateUser = userId;
                }
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Pagos aplicados correctamente"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al aplicar pagos: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessCollection",
                Action = "ApplyMultiplePayments",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<PaymentsSaleResponse> PaymentsSaleBySaleId(DetailsSaleRequest request, int userId)
    {
        PaymentsSaleResponse response = new();

        try
        {
            var sale = await Context.TPayments
                                    .Where(p => p.SaleId == request.SaleId)
                                    .Join(Context.TUsers,
                                          payment => payment.CreateUser,
                                          user => user.UserId,
                                          (payment, user) => new PaymentsSaleDTO
                                          {
                                              PaymentDate = payment.PaymentDate,
                                              Comments = payment.Comments ?? "",
                                              Amount = payment.Amount,
                                              Username = user.FirstName + " " + user.LastName,
                                              PaymentMethod = payment.PaymentMethod
                                          })
                                    .ToListAsync();

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
                Message = $"Error al obtener los pagos de la venta: {ex.Message}"
            };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessCollecion",
                Action = "PaymentsSaleBySaleId",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }

    public async Task<CreditNoteListResponse> GetCreditNotesByStatus(CreditNoteListRequest request, int userId)
    {
        CreditNoteListResponse response = new();

        try
        {
            var query = Context.TNotesCreditRequests
                               .Include(n => n.Sale)
                                   .ThenInclude(s => s.Client)
                               .Include(n => n.Sale)
                                   .ThenInclude(s => s.User)
                               .Include(n => n.CreatedByUser)
                               .Where(n => n.CreateDate.Date >= request.StartDate.Date &&
                                           n.CreateDate.Date <= request.EndDate.Date);

            if (request.ClientId.HasValue && request.ClientId.Value != 20)
                query = query.Where(n => n.Sale!.ClientId == request.ClientId.Value);

            if (request.SalesPersonId.HasValue && request.SalesPersonId.Value != 20)
                query = query.Where(n => n.Sale!.UserId == request.SalesPersonId.Value);

            if (request.SaleStatusId.HasValue)
                query = query.Where(n => n.Status == request.SaleStatusId.Value);

            var notes = await query
                .OrderByDescending(n => n.CreateDate)
                .Select(n => new CreditNoteListDTO
                {
                    NoteCreditId = n.NoteCreditId,
                    SaleId = n.SaleId,
                    FinalCreditAmount = n.FinalCreditAmount,
                    Comments = n.Comments,
                    CreateDate = n.CreateDate,
                    CreatedBy = n.CreatedByUser.FirstName + " " + n.CreatedByUser.LastName,
                    Vendedor = n.Sale!.User != null ? n.Sale.User.FirstName + " " + n.Sale.User.LastName : string.Empty,
                    ClientName = n.Sale!.Client != null ? n.Sale.Client.BusinessName : string.Empty
                })
                .ToListAsync();

            response.Result = notes;

        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO { Code = 500, Message = $"Error: {ex.Message}" };

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessCollection",
                Action = "GetCreditNotesByStatus",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }


    public async Task<ReplyResponse> ApproveCreditNoteByCollection(ApproveCreditNoteRequest request, int userId)
    {
        var response = new ReplyResponse();
        await using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var note = await Context.TNotesCreditRequests.FirstOrDefaultAsync(n => n.NoteCreditId == request.NoteCreditId && n.Status == 11);

            if (note == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "No se encontró la nota de crédito o ya fue procesada."
                };
                return response;
            }

            note.Status = 12; // Aprobada por cobranza
            note.CommentsCollection = request.CommentsCollection;
            note.UpdateUser = userId;
            note.UpdateDate = NowCDMX;

            var sale = await Context.TSales.FirstOrDefaultAsync(s => s.SaleId == note.SaleId);

            if (sale == null)
            {
                response.Error = new ErrorDTO { Code = 404, Message = "Venta no encontrada" };

                return response;
            }

            sale.SaleStatusId = 12; // Aprobada por cobranza
            sale.UpdateDate = NowCDMX;
            sale.UpdateUser = userId;

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Nota de crédito aprobada correctamente por cobranza."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-DataAccessCollection",
                Action = "ApproveCreditNoteByCollection",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Ocurrió un error al aprobar la nota de crédito."
            };
        }

        return response;
    }

    public async Task<FinanceResumeResponse> GetFinanceResume(FinanceResumeRequest request, int userId)
    {
        var response = new FinanceResumeResponse();

        try
        {
            var query = Context.TPayments
                               .Where(p => p.Status == 1 &&
                                           p.PaymentDate.Date >= request.StartDate.Date &&
                                           p.PaymentDate.Date <= request.EndDate.Date);

            if (!string.IsNullOrEmpty(request.PaymentMethod))
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod);

            var payments = await query.Select(p => new FinanceResumeDTO {
                                            PaymentId = p.PaymentId,
                                            SaleId = p.SaleId,
                                            Amount = p.Amount,
                                            PaymentMethod = p.PaymentMethod,
                                            PaymentDate = p.PaymentDate
                                       })
                                       .OrderByDescending(p => p.PaymentDate)
                                       .ToListAsync();

            response.Result = payments;
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
                Action = "GetFinanceResume",
                Message = $"Excepción: {ex.Message}",
                InnerException = ex.InnerException?.Message,
                IdUser = userId
            });
        }

        return response;
    }
}
