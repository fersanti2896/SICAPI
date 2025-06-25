namespace SICAPI.Models.Request.Warehouse;

public class CreateEntryDetailRequest
{
    public int EntryId { get; set; }
    public int ProductProviderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
