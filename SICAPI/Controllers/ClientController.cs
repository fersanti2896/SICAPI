using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.Client;

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
    /// Asigna cliente a un usuario
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ChangeClientUser")]
    public async Task<IActionResult> ChangeClientUser(UpdateClientUserRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.ChangeClientUser(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

}
