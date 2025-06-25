using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response;

public class ReplyResponse : BaseResponse
{
    public ReplyDTO? Result { get; set; }
}
