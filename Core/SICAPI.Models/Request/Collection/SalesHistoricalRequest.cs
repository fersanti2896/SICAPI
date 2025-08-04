namespace SICAPI.Models.Request.Collection;

public class SalesHistoricalRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ClientId { get; set; }
    public int? SalesPersonId { get; set; }
    public int? SaleStatusId { get; set; }
    public int? PaymentStatusId { get; set; }
}
