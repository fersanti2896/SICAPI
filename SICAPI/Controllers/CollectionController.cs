using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Finance;
using SICAPI.Models.Request.Sales;
using SICAPI.Models.Response.Collection;

namespace SICAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionRepository ICollectionRepository;

    public CollectionController(ICollectionRepository iCollectionRepository)
    {
        ICollectionRepository = iCollectionRepository;
    }

    /// <summary>
    /// Listado de estatus de pago
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllPaymentStatus")]
    public async Task<IActionResult> GetAllPaymentStatus()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetAllPaymentStatus(userId);

        return Ok(result);
    }

    /// <summary>
    /// Genera pago de un ticket
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyPayment")]
    public async Task<IActionResult> ApplyPayment(ApplyPaymentRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ICollectionRepository.ApplyPayment(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Genera pago múltiple de varios tickets
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyMultiplePayments")]
    public async Task<IActionResult> ApplyMultiplePayments(ApplyMultiplePaymentRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ICollectionRepository.ApplyMultiplePayments(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Listado de tickets para cobranza - historico
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSalesHistorical")]
    public async Task<IActionResult> GetSalesHistorical(SalesHistoricalRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetSalesHistorical(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Listado de tickets para cobranza - por cobrar
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSalesPendingPayment")]
    public async Task<IActionResult> GetSalesPendingPayment(SalesPendingPaymentRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetSalesPendingPayment(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Listado de tickets para cobranza - pagados
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSalesPaids")]
    public async Task<IActionResult> GetSalesPaids(SalesHistoricalRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetSalesPaids(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Marca ticket como Cancelado al 100% desde Cobranza para Devolución en Almacén
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CancelSaleWithComment")]
    public async Task<IActionResult> CancelSaleWithComment(CancelSaleRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.CancelSaleWithComment(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Marca ticket como Cancelado por Omision desde Cobranza
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CancelSaleByOmission")]
    public async Task<IActionResult> CancelSaleByOmission(CancelSaleRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.CancelSaleByOmission(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Obtiene listado de comentarios de la cancelación de un ticket
    /// </summary>
    /// <param name="saleId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSaleCancelledComments")]
    public async Task<IActionResult> GetSaleCancelledComments(CancelledCommentsRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetCancelledSaleComments(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Obtiene compilado para finanzas
    /// </summary>
    /// <param name="saleId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetFinanceSummaryAsync")]
    public async Task<IActionResult> GetFinanceSummaryAsync(FinanceBuildRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetFinanceSummaryAsync(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Servicio para ver los pagos de una venta por la venta id
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("PaymentsSaleBySaleId")]
    public async Task<IActionResult> PaymentsSaleBySaleId(DetailsSaleRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ICollectionRepository.PaymentsSaleBySaleId(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
