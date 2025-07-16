namespace SICAPI.Models.Request.Sales;

public class UpdateSaleStatusRequest
{
    public int SaleId { get; set; }
    public int SaleStatusId { get; set; }
    public string? Comments { get; set; }
}
