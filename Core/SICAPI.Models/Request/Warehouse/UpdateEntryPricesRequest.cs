
namespace SICAPI.Models.Request.Warehouse;

public class UpdateEntryPricesRequest
{
    public int EntryId { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public string? Observations { get; set; }
    public List<UpdateEntryPriceDetailRequest> Products { get; set; } = new();
}
