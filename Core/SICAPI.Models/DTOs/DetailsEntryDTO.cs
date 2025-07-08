namespace SICAPI.Models.DTOs;

public class DetailsEntryDTO
{
    public int EntryId { get; set; }
    public int SupplierId { get; set; }
    public string BusinessName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime ExpectedPaymentDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Observations { get; set; }
    public List<ProductsDetailsEntryDTO> ProductsDetails { get; set; }
}
