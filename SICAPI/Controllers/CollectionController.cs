using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Collection;
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

    [HttpPost]
    [Route("GetSalesPaids")]
    public async Task<IActionResult> GetSalesPaids(SalesHistoricalRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICollectionRepository.GetSalesPaids(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }
}
