using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Catalogs;
using SICAPI.Models.Response.Catalogs;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessCatalogs : IDataAccessCatalogs
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessCatalogs(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<GetStatesResponse> GetStates(int IdUser)
    {
        GetStatesResponse response = new();

        try
        {
            var states = await Context.TPostalCodes
                                       .Select(r => new { r.c_estado, r.d_estado })
                                       .Distinct()
                                       .OrderBy(r => r.d_estado)
                                       .Select(r => new StatesDTO
                                       {
                                        c_estado = r.c_estado,
                                        d_estado = r.d_estado
                                       })
                                       .ToListAsync();

            if (states != null && states.Count > 0)
                response.Result = states;
            else
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "No hay catalogo de estados"
                };
            }
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = IdUser,
                Module = "SICAPI-DataAccessCatalogs",
                Action = "GetStates",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public async Task<MunicipalityByStateResponse> GetMunicipalityByState(MunicipalityByStateRequest request, int IdUser)
    {
        MunicipalityByStateResponse response = new();

        try
        {
            var municip = await Context.TPostalCodes
                                        .Where(r => r.c_estado == request.c_estado)
                                        .Select(r => new { r.c_mnpio, r.D_mnpio })
                                        .Distinct()
                                        .OrderBy(r => r.D_mnpio)
                                        .Select(r => new MunicipalityDTO
                                        {
                                            c_mnpio = r.c_mnpio,
                                            D_mnpio = r.D_mnpio
                                        })
                                        .ToListAsync();

            if (municip != null && municip.Count > 0)
                response.Result = municip;
            else
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "No se encontró catalogo de municipios dadod el estado."
                };
            }
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = IdUser,
                Module = "SICAPI-DataAccessCatalogs",
                Action = "GetMunicipalityByState",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public async Task<TownByStateAndMunicipalityResponse> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request, int IdUser)
    {
        TownByStateAndMunicipalityResponse response = new();

        try
        {
            var colonias = await Context.TPostalCodes
                                        .Where(r => r.c_estado == request.c_estado && r.c_mnpio == request.c_mnpio)
                                        .Select(r => new { r.d_codigo, r.id_asenta_cpcons, r.d_asenta })
                                        .Distinct()
                                        .OrderBy(r => r.d_asenta)
                                        .Select(r => new TownDTO
                                        {
                                            d_codigo = r.d_codigo,
                                            id_asenta_cpcons = r.id_asenta_cpcons,
                                            d_asenta = r.d_asenta
                                        })
                                        .ToListAsync();

            if (colonias != null && colonias.Count > 0)
                response.Result = colonias;
            else
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "No se encontró colonias dado el municipio y estado."
                };
            }
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = IdUser,
                Module = "SICAPI-DataAccessCatalogs",
                Action = "GetTownByStateAndMunicipality",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }

    public async Task<CPResponse> GetCP(CPRequest request, int IdUser)
    {
        CPResponse response = new();

        try
        {
            var postalData = await Context.TPostalCodes.Where(p => p.d_codigo == request.postalCode).ToListAsync();

            if (!postalData.Any())
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "No se encontró información dado el CP"
                };
                return response;
            }

            // Tomar los datos generales del primer registro
            var baseInfo = postalData.First();

            // Obtener colonias distintas
            var neighborhoods = postalData
                .Select(r => new CPInfoDTO
                {
                    id_asenta_cpcons = r.id_asenta_cpcons,
                    d_asenta = r.d_asenta
                })
                .Distinct()
                .OrderBy(n => n.d_asenta)
                .ToList();

            response.Result = new CPDTO
            {
                id_postalCodes = baseInfo.id_postalCodes,
                d_codigo = baseInfo.d_codigo,
                c_estado = baseInfo.c_estado,
                d_estado = baseInfo.d_estado,
                c_mnpio = baseInfo.c_mnpio,
                D_mnpio = baseInfo.D_mnpio,
                neighborhoods = neighborhoods
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = IdUser,
                Module = "SICAPI-DataAccessCatalogs",
                Action = "GetCP",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
        }

        return response;
    }
}
