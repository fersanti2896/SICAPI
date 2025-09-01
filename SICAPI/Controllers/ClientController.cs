using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Client;
using SICAPI.Models.Request.Warehouse;

namespace SICAPI.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository IClientRepository;

    public ClientController(IClientRepository iClientRepository)
    {
        IClientRepository = iClientRepository;
    }

    /// <summary>
    /// Listado de clientes del sistema
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllClients")]
    public async Task<IActionResult> GetAllClients()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.GetAllClients(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de clientes de un usuario (vendedor)
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetClientsByUser")]
    public async Task<IActionResult> GetClientsByUser()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.GetClientsByUser(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de clientes de un usuario (vendedor) para ventas
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetClientsNotAddressByUser")]
    public async Task<IActionResult> GetClientsNotAddressByUser()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.GetClientsNotAddressByUser(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Crea un cliente
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreateClient")]
    public async Task<IActionResult> CreateClient(CreateClientRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IClientRepository.CreateClient(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza información de un cliente
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateClient")]
    public async Task<IActionResult> UpdateClient(UpdateClientRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.UpdateClient(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Activa/Desactiva Cliente
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("DeactivateClient")]
    public async Task<IActionResult> DeactivateClient(ActivateRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.DeactivateClient(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
