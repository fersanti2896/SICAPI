namespace SICAPI.Models.Request.Warehouse;

public class CreateEntryRequest
{
    public int SupplierId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Observations { get; set; }
}
