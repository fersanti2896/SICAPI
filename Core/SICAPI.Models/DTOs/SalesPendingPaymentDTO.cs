
namespace SICAPI.Models.DTOs;

public class SalesPendingPaymentDTO
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountPending { get; set; }
    public string SaleStatus { get; set; }
    public int PaymentStatusId { get; set; }
    public string PaymentStatus { get; set; }
    public int ClientId { get; set; }
    public string BusinessName { get; set; }
    public int SalesPersonId { get; set; }
    public string SalesPerson { get; set; }
}
