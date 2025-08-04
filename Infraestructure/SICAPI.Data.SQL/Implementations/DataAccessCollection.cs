using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Collection;
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
                Module = "SICAPI-DataAccessSales",
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
                Module = "SICAPI-DataAccessSales",
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
                Module = "SICAPI-DataAccessSales",
                Action = "GetSalesPaids",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });
        }

        return response;
    }
}
