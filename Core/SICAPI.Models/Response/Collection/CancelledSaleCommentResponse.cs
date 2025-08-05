using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Collection;

public class CancelledSaleCommentResponse : BaseResponse
{
    public List<CancelledSaleCommentDTO>? Result { get; set; }
}
