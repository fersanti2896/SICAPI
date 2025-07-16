
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessLogs : IDataAccessLogs
{
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessLogs(AppDbContext appDbContext)
    {
        Context = appDbContext;
    }

    public async Task<bool> Create(LogsDTO logDTO)
    {
        bool flagLog;
        try
        {
            var log = new TLog
            {
                Module = logDTO.Module,
                Action = logDTO.Action,
                Message = logDTO.Message,
                InnerException = logDTO.InnerException,
                UserId = logDTO.IdUser,
                CreateUser = logDTO.IdUser,
                UpdateUser = logDTO.IdUser,
                Status = 1,
                CreateDate = NowCDMX,
                UpdateDate = NowCDMX
            };

            Context.TLogs!.Add(log);
            flagLog = await Context.SaveChangesAsync() > 0;
        }
        catch (Exception)
        {
            flagLog = false;
        }

        return flagLog;
    }
}
