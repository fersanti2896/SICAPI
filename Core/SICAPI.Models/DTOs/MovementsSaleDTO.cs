namespace SICAPI.Models.DTOs;

public class MovementsSaleDTO
{
    public int SaleId { get; set; }
    public string? Comments { get; set; }
    public string? CommentsDelivery { get; set; }
    public DateTime? UpdateDate { get; set; }
}
