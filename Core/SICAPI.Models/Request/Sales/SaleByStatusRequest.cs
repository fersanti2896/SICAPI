namespace SICAPI.Models.Request.Sales;

public class SaleByStatusRequest
{
    public int SaleStatusId { get; set; }
    public int? ClientId { get; set; }
    public int? SalesPersonId { get; set; }
}
