namespace SICAPI.Models.DTOs;

public class LogsDTO
{
    public int IdUser { get; set; }
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? Message { get; set; }
    public string? InnerException { get; set; }
    public string? RowNumber { get; set; }
}
