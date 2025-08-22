using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TNotesCreditRequests")]
public class TNotesCreditRequests : TDataGeneric
{
    [Key]
    public int NoteCreditId { get; set; }

    public int SaleId { get; set; }
    public int ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public bool IsApproved { get; set; }
    public decimal FinalCreditAmount { get; set; }
    public string? Comments { get; set; }
    public string? CommentsCollection { get; set; }
    public string? CommentsDevolution { get; set; }


    [ForeignKey("SaleId")]
    public virtual TSales? Sale { get; set; }
    [ForeignKey("CreateUser")]
    public virtual TUsers? CreatedByUser { get; set; }
    public virtual ICollection<TNotesCreditDetails> Details { get; set; } = new List<TNotesCreditDetails>();
}
