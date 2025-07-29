namespace SICAPI.Models.Request.Sales;

public class AssignDeliveryUserRequest
{
    public int SaleId { get; set; }
    public int DeliveryUserId { get; set; }
    public string CommentsDelivery { get; set; }
    public bool IsUpdated { get; set; }
}
