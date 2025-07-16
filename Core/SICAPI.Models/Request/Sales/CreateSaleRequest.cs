
using SICAPI.Models.DTOs;

namespace SICAPI.Models.Request.Sales;

public class CreateSaleRequest
{
    public int ClientId { get; set; }
    public List<ProductSaleDTO> Products { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
