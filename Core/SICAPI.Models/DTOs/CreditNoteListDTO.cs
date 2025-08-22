namespace SICAPI.Models.DTOs;

public class CreditNoteListDTO
{
    public int NoteCreditId { get; set; }
    public int SaleId { get; set; }
    public decimal FinalCreditAmount { get; set; }
    public string? Comments { get; set; }
    public DateTime CreateDate { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ClientName { get; set; }
}
