
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Warehouse;

public class DetailsEntryResponse : BaseResponse
{
    public DetailsEntryDTO? Result { get; set; }
}
