namespace SICAPI.Models.Request.Sales;

public class SalesByUserRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int SaleStatusId { get; set; }
}
