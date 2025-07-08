namespace SICAPI.Models.Request.Warehouse;

public class UpdateProductRequest
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? Presentation { get; set; }
    public string? Unit { get; set; }
    public decimal? Price { get; set; }
}
