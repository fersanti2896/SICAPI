using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Warehouse;

public class EntryResponse : BaseResponse
{
    public EntryDTO? Result { get; set; }
}
