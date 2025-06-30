namespace SICAPI.Models.DTOs;

public class EntrySummaryDTO
{
    public int SupplierId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public decimal TotalAmount { get; set; }
}
