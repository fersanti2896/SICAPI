using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TNotesCreditDetails")]
public class TNotesCreditDetails
{
    [Key]
    public int NoteCreditDetailId { get; set; }

    public int NoteCreditId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal SubTotal { get; set; }

    [ForeignKey("NoteCreditId")]
    public virtual TNotesCreditRequests? NoteCredit { get; set; }

    [ForeignKey("ProductId")]
    public virtual TProducts? Product { get; set; }
}