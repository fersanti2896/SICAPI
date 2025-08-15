using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Implementations;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.Request.User;

namespace SICAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IUserRepository IUserRepository;

    public UserController(IUserRepository iUserRepository)
    {
        IUserRepository = iUserRepository;
    }

    /// <summary>
    /// Servicio que crea un usuario con rol
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("CreateUser")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await IUserRepository.CreateUser(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Servicio de logueo de usuario
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await IUserRepository.Login(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Servicio de refrescar el token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await IUserRepository.RefreshToken(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Listado de usuarios del sistema
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IUserRepository.GetAllUsers(userId);

        return Ok(result);
    }

    /// <summary>
    /// Activa/Desactiva Usuario
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("DeactivateUser")]
    public async Task<IActionResult> DeactivateUser(ActivateUserRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IUserRepository.DeactivateUser(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Información de Credito de un usuario
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("CreditInfo")]
    public async Task<IActionResult> CreditInfo()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IUserRepository.CreditInfo(userId);

        return Ok(result);
    }

    /// <summary>
    /// Servicio que crea un usuario con rol
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("UpdateUser")]
    public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IUserRepository.UpdateUser(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
}
