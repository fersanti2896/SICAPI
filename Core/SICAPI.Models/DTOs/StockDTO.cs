namespace SICAPI.Models.DTOs;

public class StockDTO
{
    public int InventoryId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public int CurrentStock { get; set; }
    public int? Apartado { get; set; }
    public int? StockReal { get; set; }
    public DateTime? LastUpdateDate { get; set; }
}
