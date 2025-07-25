using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Warehouse;

namespace SICAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IProductRepository IProductRepository;

    public WarehouseController(IProductRepository iProductRepository)
    {
        IProductRepository = iProductRepository;
    }

    /// <summary>
    /// Inventario Stock
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetStock")]
    public async Task<IActionResult> GetStock()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.GetStock(userId);

        return Ok(result);
    }

    /// <summary>
    /// Productos de Stock Real
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetStockReal")]
    public async Task<IActionResult> GetStockReal()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.GetStockReal(userId);

        return Ok(result);
    }

    /// <summary>
    /// Listado de productos del sistema
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetAllProducts")]
    public async Task<IActionResult> GetAllProducts()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.GetAllProducts(userId);

        return Ok(result);
    }

    /// <summary>
    /// Crea un producto para SIC
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateProduct")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.CreateProduct(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza un producto
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateProduct")]
    public async Task<IActionResult> UpdateProduct(UpdateProductRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.UpdateProduct(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Activa/Desactiva Producto
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("DeactivateProduct")]
    public async Task<IActionResult> DeactivateProduct(ActiveProductRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.DeactivateProduct(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Crea la relacion entre el producto y el proveedor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateProductProvider")]
    public async Task<IActionResult> CreateProductProvider(CreateProductProviderRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.CreateProductProvider(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Crea la nota de pedido con sus productos vinculados
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateFullEntry")]
    public async Task<IActionResult> CreateFullEntry(CreateEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.CreateFullEntry(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Detalles de la nota de pedido
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("FullEntryById")]
    public async Task<IActionResult> FullEntryById(DetailsEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.FullEntryById(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza información de una nota de compra (precios de productos)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateFullEntry")]
    public async Task<IActionResult> UpdateFullEntry(UpdateEntryPricesRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.UpdateEntryPrices(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
