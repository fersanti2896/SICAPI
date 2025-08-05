using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Entities;

[Table("TCancelledSalesComments")]
public class TCancelledSalesComments : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CancelledSaleCommentId { get; set; }
    public int SaleId { get; set; }
    public string Comments { get; set; } = null!;


    [ForeignKey("SaleId")]
    public virtual TSales? Sale { get; set; }
}
