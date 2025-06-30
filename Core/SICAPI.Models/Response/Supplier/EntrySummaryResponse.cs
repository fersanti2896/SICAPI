using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Supplier;

public class EntrySummaryResponse : BaseResponse
{
    public List<EntrySummaryDTO>? Result { get; set; }
}
