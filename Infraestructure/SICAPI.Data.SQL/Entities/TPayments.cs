using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TPayments")]
public class TPayments : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public string? Comments { get; set; }
    public int? ThirdPartySupplierId { get; set; }



    [ForeignKey("SaleId")]
    public virtual TSales? Sale { get; set; }

    [ForeignKey("ThirdPartySupplierId")]
    public virtual TSuppliers? ThirdPartySupplier { get; set; }
}
