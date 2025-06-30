
namespace SICAPI.Models.Request.Warehouse;

public class CreateProductRequest
{
    public string ProductName { get; set; }
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
}
