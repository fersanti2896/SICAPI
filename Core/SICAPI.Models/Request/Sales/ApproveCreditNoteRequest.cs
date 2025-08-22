namespace SICAPI.Models.Request.Sales;

public class ApproveCreditNoteRequest
{
    public int NoteCreditId { get; set; }           // ID de la nota de crédito a aprobar
    public string CommentsCollection { get; set; }
}
