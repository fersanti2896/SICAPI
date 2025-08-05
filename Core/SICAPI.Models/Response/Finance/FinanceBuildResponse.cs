
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Finance;

public class FinanceBuildResponse : BaseResponse
{
    public List<FinanceMethodTotalDTO>? Result { get; set; }
}
