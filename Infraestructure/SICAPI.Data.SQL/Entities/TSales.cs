
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;


[Table("TSales")]
public class TSales : TDataGeneric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SaleId { get; set; }
    public int ClientId { get; set; }                // ID del cliente
    public int UserId { get; set; }                  // ID del vendedor (usuario que genera la venta)
    public DateTime SaleDate { get; set; }           // Fecha en la que se realiza la venta
    public decimal TotalAmount { get; set; }         // Monto total de la venta
    public int SaleStatusId { get; set; }            // Estatus actual del ticket
    public int? DeliveryUserId { get; set; }
    public string? Comments { get; set; }


    [ForeignKey("ClientId")]
    public virtual TClients? Client { get; set; }

    [ForeignKey("UserId")]
    public virtual TUsers? User { get; set; }

    [ForeignKey("SaleStatusId")]
    public virtual TSaleStatuses? SaleStatus { get; set; }

    [ForeignKey("DeliveryUserId")]
    public virtual TUsers? DeliveryUser { get; set; }

    public virtual ICollection<TSalesDetail>? SaleDetails { get; set; }
}
