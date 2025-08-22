
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Request.Sales;

public class CreditNoteRequest
{
    public int SaleId { get; set; }
    public string? Comments { get; set; }
    public List<CreditNoteProductDTO> Products { get; set; } = new();
}
