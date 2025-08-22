using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Collection;
using SICAPI.Models.Request.Sales;

namespace SICAPI.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesRepository ISalesRepository;

    public SalesController(ISalesRepository iSalesRepository)
    {
        ISalesRepository = iSalesRepository;
    }

    /// <summary>
    /// Servicio para crear una venta. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateSale")]
    public async Task<IActionResult> CreateSale(CreateSaleRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.CreateSale(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de ventas-tickets del sistema
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("GetAllSalesByStatus")]
    public async Task<IActionResult> GetAllSales(SaleByStatusRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISalesRepository.GetAllSalesByStatus(request, userId);

        return Ok(result);
    }

    /// <summary>
    /// Servicio para los detalles de una venta por la venta id
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("DetailsSaleBySaleId")]
    public async Task<IActionResult> DetailsSaleBySaleId(DetailsSaleRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.DetailsSaleBySaleId(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de estatus de ticket
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllSalesStatus")]
    public async Task<IActionResult> GetAllSalesStatus()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISalesRepository.GetAllSalesStatus(userId);

        return Ok(result);
    }

    /// <summary>
    /// Asigna el ticket a un vendedor/repartidor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("AssignDeliveryUser")]
    public async Task<IActionResult> AssignDeliveryUser(AssignDeliveryUserRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.AssignDeliveryUser(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Actualiza estatus del ticket
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateSaleStatus")]
    public async Task<IActionResult> UpdateSaleStatus(UpdateSaleStatusRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.UpdateSaleStatus(request, userId);

        return result.Error != null ? BadRequest(result) : Ok(result);
    }

    /// <summary>
    /// Servicio para ver los movimientos de una venta por la venta id
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("MovementsSaleBySaleId")]
    public async Task<IActionResult> MovementsSaleBySaleId(DetailsSaleRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.MovementsSaleBySaleId(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de tickets para estatus por propio (repartidor)
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSalesByDeliveryId")]
    public async Task<IActionResult> GetSalesByDeliveryId(SaleByStatusRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISalesRepository.GetSalesByDeliveryId(request, userId);

        return Ok(result);
    }

    /// <summary>
    /// Listado de tickets | ventas por un usuario con rango de fechas (por default 15 dias)
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("GetSalesByUser")]
    public async Task<IActionResult> GetSalesByUser(SalesByUserRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISalesRepository.GetSalesByUser(request, userId);

        return Ok(result);
    }

    /// <summary>
    /// Confirma cancelación de ticket por parte de almacen y retorna todos los productos al stock
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("ConfirmCancellationSale")]
    public async Task<IActionResult> ConfirmCancellationSale(CancelSaleRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISalesRepository.ConfirmReturnAndRevertStock(request, userId);

        return Ok(result);
    }

    /// <summary>
    /// Crea una nota de crédito de un ticket
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateCreditNoteRequest")]
    public async Task<IActionResult> CreateCreditNoteRequest(CreditNoteRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.CreateCreditNoteRequest(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Servicio para los detalles de una nota de crédito por la noteCreditId
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("DetailsNoteCreditById")]
    public async Task<IActionResult> DetailsNoteCreditById(DetailsNoteCreditRequest request)
    {
        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await ISalesRepository.DetailsNoteCreditById(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
