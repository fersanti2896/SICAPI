

using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Finance;

public class FinanceResumeResponse : BaseResponse
{
    public List<FinanceResumeDTO>? Result { get; set; }
}
