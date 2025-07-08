
namespace SICAPI.Models.DTOs;

public class ProductsDetailsEntryDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPice { get; set; }
    public decimal SubTotal { get; set; }
    public string? Lot { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
