
namespace SICAPI.Models.DTOs;

public class DetailsNoteCreditDTO
{
    public int NoteCreditId { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public DateTime CreateDate { get; set; }
    public int CreateUser { get; set; }
    public string CreadoPor { get; set; }
    public decimal FinalCreditAmount { get; set; }
}
