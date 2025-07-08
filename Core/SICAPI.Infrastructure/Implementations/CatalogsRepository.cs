using Azure;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Request.Catalogs;
using SICAPI.Models.Response;
using SICAPI.Models.Response.Catalogs;

namespace SICAPI.Infrastructure.Implementations;

public class CatalogsRepository : ICatalogsRepository
{
    private readonly IDataAccessCatalogs IDataAccessCatalogs;
    private IDataAccessLogs IDataAccessLogs;

    public CatalogsRepository(IDataAccessCatalogs iDataAccessCatalogs, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessCatalogs = iDataAccessCatalogs;
        IDataAccessLogs = iDataAccessLogs;
    }

    public async Task<MunicipalityByStateResponse> GetMunicipalityByState(MunicipalityByStateRequest request, int IdUser)
    {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetMunicipalityByState(request, IdUser), "GetMunicipalityByState", IdUser);
    }

    public async Task<TownByStateAndMunicipalityResponse> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request, int IdUser)
    {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetTownByStateAndMunicipality(request, IdUser), "GetTownByStateAndMunicipality", IdUser);
    }

    public async Task<CPResponse> GetCP(CPRequest request, int IdUser) {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetCP(request, IdUser), "GetCP", IdUser);
    }

    public async Task<GetStatesResponse> GetStates(int IdUser)
    {
        GetStatesResponse response = new();

        try
        {
            response = await IDataAccessCatalogs.GetStates(IdUser);

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "SICAPI-CatalogsRepository",
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

            return response;
        }
    }

    private async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string actionName, int userId) where T : BaseResponse, new()
    {
        T response = new();

        try
        {
            response = await action();
            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = userId,
                Module = "SICAPI-CatalogsRepository",
                Action = actionName,
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };
            return response;
        }
    }
}
