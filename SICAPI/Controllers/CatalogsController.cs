using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Catalogs;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Catalogs;
using System.Net;

namespace SICAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly ICatalogsRepository ICatalogsRepository;

    public CatalogsController(ICatalogsRepository iCatalogsRepository)
    {
        ICatalogsRepository = iCatalogsRepository;
    }

    /// <summary>
    /// Obtiene los estados
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetStates")]
    public async Task<IActionResult> GetStates()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICatalogsRepository.GetStates(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Obtienes los municipios por estado.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetMunicipalityByState")]
    public async Task<IActionResult> GetMunicipalityByState(MunicipalityByStateRequest request)
    {
        return await HandlePostRequest<MunicipalityByStateRequest, MunicipalityByStateResponse>(request, ICatalogsRepository.GetMunicipalityByState);
    }

    /// <summary>
    /// Obtiene las colonias por municipio y estado.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetTownByStateAndMunicipality")]
    public async Task<IActionResult> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request)
    {
        return await HandlePostRequest<TownByStateAndMunicipalityRequest, TownByStateAndMunicipalityResponse>(request, ICatalogsRepository.GetTownByStateAndMunicipality);
    }

    /// <summary>
    /// Obtiene información de un CP.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetCP")]
    public async Task<IActionResult> GetCP(CPRequest request)
    {
        return await HandlePostRequest<CPRequest, CPResponse>(request, ICatalogsRepository.GetCP);
    }

    #region
    /// <summary>
    /// Metodo que sirve para evaluar si se envian las propiedades del request, así como la función que irá a BusinessEnrollment
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="businessLogicMethod"></param>
    /// <returns></returns>
    private async Task<IActionResult> HandlePostRequest<TRequest, TResponse>(TRequest request, Func<TRequest, int, Task<TResponse>> businessLogicMethod)
    where TResponse : BaseResponse
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            var errorMessage = string.Join(" ", errors);

            return BadRequest(new ErrorDTO
            {
                Code = 400,
                Message = errorMessage
            });
        }

        int IdUser = Convert.ToInt32(User.Claims.Where(x => x.Type == "UserId").First().Value);
        var result = await businessLogicMethod(request, IdUser);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion
}
