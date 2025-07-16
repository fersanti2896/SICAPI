using SICAPI.Models.DTOsc;

namespace SICAPI.Models.Response.User;

public class UserInfoResponse : BaseResponse
{
    public UserCreditInfoDTO? Result { get; set; }
}
