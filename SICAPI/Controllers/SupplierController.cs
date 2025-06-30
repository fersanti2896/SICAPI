using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Supplier;
using SICAPI.Models.Request.User;
using SICAPI.Models.Request.Warehouse;

namespace SICAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierRepository ISupplierRepository;

    public SupplierController(ISupplierRepository supplierRepository)
    {
        ISupplierRepository = supplierRepository;
    }

    /// <summary>
    /// Crea a un proveedor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateSupplier")]
    public async Task<IActionResult> CreateSupplier(CreateSupplierRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");

        if (userIdClaim == null)
            return Unauthorized("UserId not found in token.");

        int userId = int.Parse(userIdClaim.Value);

        var result = await ISupplierRepository.CreateSupplier(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza información de un proveedor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateSupplier")]
    public async Task<IActionResult> UpdateSupplier(UpdateSupplierRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");

        if (userIdClaim == null)
            return Unauthorized("UserId not found in token.");

        int userId = int.Parse(userIdClaim.Value);

        var result = await ISupplierRepository.UpdateSupplier(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de proveedores del sistema
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetAllSuppliers")]
    public async Task<IActionResult> GetAllSuppliers()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISupplierRepository.GetAllSuppliers(userId);

        return Ok(result);
    }

    /// <summary>
    /// Listado de notas de pedido
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetEntryList")]
    public async Task<IActionResult> GetEntryList()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISupplierRepository.GetEntryList(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Activa/Desactiva Proveedor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("DeactivateSupplier")]
    public async Task<IActionResult> DeactivateSupplier(ActivateRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ISupplierRepository.DeactivateSupplier(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
