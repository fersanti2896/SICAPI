namespace SICAPI.Models.DTOs;

public class DetailsSaleDTO
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public string? Lot { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreateDate { get; set; }
    public string Vendedor { get; set; }
}
