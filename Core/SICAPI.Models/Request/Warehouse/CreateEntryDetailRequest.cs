namespace SICAPI.Models.Request.Warehouse;

public class CreateEntryDetailRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Lot { get; set; }
}
