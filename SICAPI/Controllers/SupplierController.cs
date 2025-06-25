using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Supplier;

namespace SICAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierController(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
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

        var result = await _supplierRepository.CreateSupplier(request, userId);

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

        var result = await _supplierRepository.UpdateSupplier(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
