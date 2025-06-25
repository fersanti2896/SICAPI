
namespace SICAPI.Models.Request.Warehouse;

public class CreateProductProviderRequest
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public string ProviderKey { get; set; } = "AAAA";
    public string? ProviderDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Unit { get; set; }
}
