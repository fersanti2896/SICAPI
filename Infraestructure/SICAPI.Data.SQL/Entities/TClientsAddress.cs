using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TClientsAddress")]
public class TClientsAddress : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClientAddressId { get; set; }
    public int ClientId { get; set; }
    public string? Cve_CodigoPostal { get; set; }
    public string? Cve_Estado { get; set; }
    public string? Cve_Municipio { get; set; }
    public string? Cve_Colonia { get; set; }
    public string? Street { get; set; }
    public string? ExtNbr { get; set; }
    public string? InnerNbr { get; set; }

    [ForeignKey("ClientId")]
    public virtual TClients? Clients { get; set; }
}
