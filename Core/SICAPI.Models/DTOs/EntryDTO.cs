namespace SICAPI.Models.DTOs;

public class EntryDTO
{
    public int EntryId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime EntryDate { get; set; }
}
