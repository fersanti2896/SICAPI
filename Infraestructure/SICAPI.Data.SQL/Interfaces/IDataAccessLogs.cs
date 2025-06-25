using SICAPI.Models.DTOs;

namespace SICAPI.Data.SQL.Interfaces;

public interface IDataAccessLogs
{
    Task<bool> Create(LogsDTO logDTO);
}
