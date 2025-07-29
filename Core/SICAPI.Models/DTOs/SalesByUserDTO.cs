
namespace SICAPI.Models.DTOs;

public class SalesByUserDTO
{
    public int SaleId { get; set; }
    public int ClientId { get; set; }
    public string BusinessName { get; set; }
    public int UserId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int SaleStatusId { get; set; }
    public string StatusName { get; set; }
    public int PaymentStatusId { get; set; }
    public string NamePayment { get; set; }
}
