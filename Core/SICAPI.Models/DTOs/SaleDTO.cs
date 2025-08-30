namespace SICAPI.Models.DTOs;

public class SaleDTO
{
    public int SaleId { get; set; }
    public int ClientId { get; set; }
    public string BusinessName { get; set; }
    public int SaleStatusId { get; set; }
    public string StatusName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public int SalesPersonId { get; set; }
    public string? Vendedor { get; set; }
    public string? Repartidor { get; set; }
}
