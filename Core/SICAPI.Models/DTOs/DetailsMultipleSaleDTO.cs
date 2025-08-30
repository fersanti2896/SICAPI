
namespace SICAPI.Models.DTOs;

public class DetailsMultipleSaleDTO
{
    public int SaleId { get; set; }
    public DateTime CreateDate { get; set; }
    public string BussinessName { get; set; }
    public string Vendedor { get; set; }
    public decimal TotalAmount { get; set; }
    public List<DetailsSaleDTO> Products { get; set; }
}
