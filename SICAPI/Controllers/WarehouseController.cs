using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Implementations;
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
    /// Crea el header de la nota de pedido
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateEntry")]
    public async Task<IActionResult> CreateEntry(CreateEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IProductRepository.CreateEntry(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Crea los detalles de la nota de pedido (Productos)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateEntryDetail")]
    public async Task<IActionResult> CreateEntryDetail(CreateEntryDetailRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IProductRepository.CreateEntryDetail(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
