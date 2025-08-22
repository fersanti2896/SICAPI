namespace SICAPI.Models.Request.Sales;

public class CreditNoteListRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ClientId { get; set; }
    public int? SalesPersonId { get; set; }
    public int? SaleStatusId { get; set; }
}
