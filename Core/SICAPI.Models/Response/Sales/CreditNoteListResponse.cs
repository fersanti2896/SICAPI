using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Sales;

public class CreditNoteListResponse : BaseResponse
{
    public List<CreditNoteListDTO>? Result { get; set; }
}
