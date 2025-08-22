namespace SICAPI.Models.Request.Warehouse;

public class ConfirmCreditNoteRequest
{
    public int NoteCreditId { get; set; } // ID de la nota de crédito
    public string? CommentsDevolution { get; set; } // Comentario del almacen
}
