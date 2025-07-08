using SICAPI.Models.Request.Catalogs;
using SICAPI.Models.Response.Catalogs;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessCatalogs
{
    Task<GetStatesResponse> GetStates(int IdUser);
    Task<MunicipalityByStateResponse> GetMunicipalityByState(MunicipalityByStateRequest request, int IdUser);
    Task<TownByStateAndMunicipalityResponse> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request, int IdUser);
    Task<CPResponse> GetCP(CPRequest request, int IdUser);
}
